using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using Competition.Application.Auth.Interfaces;
using Competition.Application.Common.Models;
using Competition.Application.Helpers;
using Competition.Application.Interfaces;
using Competition.Application.Payments;
using Competition.Domain.Entities;
using Competition.Domain.Entities.MatchEntities;
using Competition.Domain.Entities.Payments;
using Competition.Domain.Entities.Person;
using Competition.Infrastructure.Data;
using Competition.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NodaTime;

namespace Competition.Tests.IntegrationTests;

public class PremiumFlowTests(WebApplicationFactory<Program> factory) : BaseIntegrationTest(factory.WithWebHostBuilder(builder =>
    builder.ConfigureTestServices(services =>
    {
        services.RemoveAll<IPaymentGateway>();
        services.AddScoped<IPaymentGateway, FakeGateway>();
    })))
{
    private const string Id = "0123456789abcdef0123456789abcdef";

    private sealed class FakeGateway : IPaymentGateway
    {
        public PremiumPricingDto GetPricing() => new(499, "eur");

        public Task<CheckoutSession> CreateCheckoutAsync(string? customerEmail, int months, CancellationToken cancellationToken) =>
            Task.FromResult(new CheckoutSession("cs_" + Guid.NewGuid().ToString("N"), "https://stripe.test/pay", 499 * months, "eur"));

        public PaymentWebhookEvent? ParseWebhook(string payload, string signature) => signature == "good"
            ? new PaymentWebhookEvent(PaymentWebhookOutcome.Paid, payload, "pi_1")
            : throw BusinessRule.Violation("Signature", "The Stripe signature is not valid.");
    }

    private async Task<ApplicationUser> SignIn(Instant? premiumUntil = null)
    {
        using var scope = ServiceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(), Email = $"{Guid.NewGuid():N}@example.invalid", UserName = Guid.NewGuid().ToString("N"),
            PremiumUntil = premiumUntil,
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
            scope.ServiceProvider.GetRequiredService<IJwtTokenGenerator>().GenerateJwtToken(user, ["User"]));
        return user;
    }

    private static Instant Days(int days) => SystemClock.Instance.GetCurrentInstant() + Duration.FromDays(days);

    [Theory]
    [InlineData("/api/statistic/advanced/team-form?selectionPublicId=" + Id)]
    [InlineData("/api/statistic/advanced/head-to-head?firstSelectionPublicId=" + Id + "&secondSelectionPublicId=fedcba9876543210fedcba9876543210")]
    [InlineData("/api/statistic/advanced/players")]
    public async Task AdvancedEndpoints_ArePremiumOnly(string url)
    {
        (await Client.GetAsync(url)).StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        await SignIn();
        (await Client.GetAsync(url)).StatusCode.Should().Be(HttpStatusCode.Forbidden);

        await SignIn(Days(-1));
        (await Client.GetAsync(url)).StatusCode.Should().Be(HttpStatusCode.Forbidden);

        await SignIn(Days(1));
        (await Client.GetAsync(url)).StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Checkout_NeedsLogin_AndAnOfferedPlan()
    {
        (await Client.PostAsJsonAsync("/api/payment/checkout", new { months = 1 })).StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        await SignIn();
        (await Client.PostAsJsonAsync("/api/payment/checkout", new { months = 2 })).StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PaidWebhook_SavesTheTransaction_AndUnlocksTheAdvancedEndpoints()
    {
        var user = await SignIn();
        var checkout = await Client.PostAsJsonAsync("/api/payment/checkout", new { months = 3 });
        checkout.StatusCode.Should().Be(HttpStatusCode.OK);
        (await checkout.Content.ReadFromJsonAsync<CheckoutSessionDto>())!.Url.Should().Be("https://stripe.test/pay");

        string sessionId;
        using (var scope = ServiceProvider.CreateScope())
        {
            var payment = await scope.ServiceProvider.GetRequiredService<AppDbContext>().PaymentTransactions.SingleAsync(x => x.UserId == user.Id);
            payment.Status.Should().Be(PaymentStatus.Pending);
            (payment.AmountCents, payment.Currency, payment.Months).Should().Be((1497, "eur", 3));
            sessionId = payment.StripeSessionId;
        }
        (await Client.GetAsync("/api/statistic/advanced/players")).StatusCode.Should().Be(HttpStatusCode.Forbidden);

        var anonymous = new HttpRequestMessage(HttpMethod.Post, "/api/payment/webhook") { Content = new StringContent(sessionId, Encoding.UTF8, "application/json") };
        anonymous.Headers.Add("Stripe-Signature", "bad");
        (await Client.SendAsync(anonymous)).StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var webhook = new HttpRequestMessage(HttpMethod.Post, "/api/payment/webhook") { Content = new StringContent(sessionId, Encoding.UTF8, "application/json") };
        webhook.Headers.Add("Stripe-Signature", "good");
        (await Client.SendAsync(webhook)).StatusCode.Should().Be(HttpStatusCode.OK);

        (await Client.GetAsync("/api/statistic/advanced/players")).StatusCode.Should().Be(HttpStatusCode.OK);
        var status = (await Client.GetFromJsonAsync<PremiumStatusDto>("/api/payment/status"))!;
        status.IsPremium.Should().BeTrue();
        status.LatestPaymentStatus.Should().Be("Paid");
        status.PremiumUntil.Should().NotBeNull();

        using var check = ServiceProvider.CreateScope();
        var saved = await check.ServiceProvider.GetRequiredService<AppDbContext>().PaymentTransactions.Include(x => x.User).SingleAsync(x => x.UserId == user.Id);
        saved.Status.Should().Be(PaymentStatus.Paid);
        saved.StripePaymentIntentId.Should().Be("pi_1");
        saved.PaidAt.Should().NotBeNull();
        (saved.User.PremiumUntil > Days(89)).Should().BeTrue();
    }

    [Fact]
    public void Jwt_CarriesThePremiumExpiry()
    {
        using var scope = ServiceProvider.CreateScope();
        var jwt = scope.ServiceProvider.GetRequiredService<IJwtTokenGenerator>();
        var until = Instant.FromUtc(2027, 1, 1, 0, 0);

        Claims(jwt.GenerateJwtToken(new ApplicationUser { Id = Guid.NewGuid(), Email = "a@example.invalid", PremiumUntil = until }, []))
            .Should().ContainSingle(x => x.Type == "premium_until" && x.Value == "2027-01-01T00:00:00Z");
        Claims(jwt.GenerateJwtToken(new ApplicationUser { Id = Guid.NewGuid(), Email = "a@example.invalid" }, []))
            .Should().NotContain(x => x.Type == "premium_until");

        static IEnumerable<System.Security.Claims.Claim> Claims(string token) => new JwtSecurityTokenHandler().ReadJwtToken(token).Claims;
    }

    [Fact]
    public async Task PlayerStats_CountPenaltiesAsGoals_AndSkipOtherEvents()
    {
        await SignIn(Days(1));
        var selection = await SeedPlayerEvents();

        var page = (await Client.GetFromJsonAsync<PagedResult<PlayerStatsRow>>($"/api/statistic/advanced/players?selectionPublicId={selection}"))!;

        page.TotalCount.Should().Be(1);
        var row = page.Items.Single();
        row.PlayerName.Should().Be("Edin Dzeko");
        (row.Goals, row.YellowCards, row.RedCards).Should().Be((2, 1, 0));
    }

    private async Task<string> SeedPlayerEvents()
    {
        using var scope = ServiceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var match = StatisticTestData.NewMatch();
        match.Status!.Slug = "finished";
        (match.HomeScore, match.AwayScore) = (2, 0);
        db.Matches.Add(match);
        var player = new Player { FirstName = "Edin", LastName = "Dzeko", Picture = [] };
        db.Players.Add(player);
        var part = new MatchPart { Code = "test-first-half", Name = "1st half" };
        Event New(string code) => new()
        {
            Match = match, Selection = match.HomeSelection!, Player = player, MatchPart = part, Minute = 10,
            EventType = new EventType { Code = code, Name = code },
        };
        db.Events.AddRange(New("goal"), New("penalty"), New("yellow-card"), New("substitution"), New("own-goal"));
        await db.SaveChangesAsync();
        return match.HomeSelection!.PublicId;
    }
}
