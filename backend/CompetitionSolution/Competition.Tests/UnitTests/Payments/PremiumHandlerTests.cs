using Competition.Application.Commands.PaymentCommands;
using Competition.Application.Commands.PaymentCommands.Validators;
using Competition.Application.Interfaces;
using Competition.Application.Payments;
using Competition.Application.Queries.Statistics.Advanced;
using Competition.Domain.Entities;
using Competition.Domain.Entities.Payments;
using FluentAssertions;
using FluentValidation;
using Moq;
using NodaTime;

namespace Competition.Tests.UnitTests.Payments;

public class PremiumHandlerTests
{
    private static readonly Instant Now = Instant.FromUtc(2026, 10, 30, 12, 0);
    private readonly Mock<IPaymentStore> store = new();
    private readonly Mock<IPaymentGateway> gateway = new();
    private readonly Mock<IUnitOfWork> unit = new();

    private HandleStripeWebhookCommandHandler Handler()
    {
        var clock = new Mock<IClock>();
        clock.Setup(x => x.GetCurrentInstant()).Returns(Now);
        return new(store.Object, gateway.Object, unit.Object, clock.Object);
    }

    private PaymentTransaction Payment(Instant? premiumUntil = null, PaymentStatus status = PaymentStatus.Pending, int months = 1)
    {
        var payment = new PaymentTransaction
        {
            StripeSessionId = "cs_1", Months = months, Status = status,
            User = new ApplicationUser { Id = Guid.NewGuid(), PremiumUntil = premiumUntil },
        };
        store.Setup(x => x.FindBySessionAsync("cs_1", It.IsAny<CancellationToken>())).ReturnsAsync(payment);
        return payment;
    }

    private void Event(PaymentWebhookOutcome outcome) => gateway.Setup(x => x.ParseWebhook("{}", "sig"))
        .Returns(new PaymentWebhookEvent(outcome, "cs_1", "pi_1"));

    [Fact]
    public async Task PaidEvent_ActivatesPremium_AndRecordsTheTransaction()
    {
        var payment = Payment();
        Event(PaymentWebhookOutcome.Paid);

        await Handler().Handle(new HandleStripeWebhookCommand("{}", "sig"), default);

        payment.Status.Should().Be(PaymentStatus.Paid);
        payment.PaidAt.Should().Be(Now);
        payment.StripePaymentIntentId.Should().Be("pi_1");
        payment.User.PremiumUntil.Should().Be(Now + Duration.FromDays(30));
        unit.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PaidEvent_ExtendsFromTheCurrentExpiry_WhileStillPremium()
    {
        var current = Now + Duration.FromDays(10);
        var payment = Payment(current, months: 3);
        Event(PaymentWebhookOutcome.Paid);

        await Handler().Handle(new HandleStripeWebhookCommand("{}", "sig"), default);

        payment.User.PremiumUntil.Should().Be(current + Duration.FromDays(90));
    }

    [Fact]
    public async Task PaidEvent_RestartsFromNow_WhenExpired()
    {
        var payment = Payment(Now - Duration.FromDays(10));
        Event(PaymentWebhookOutcome.Paid);

        await Handler().Handle(new HandleStripeWebhookCommand("{}", "sig"), default);

        payment.User.PremiumUntil.Should().Be(Now + Duration.FromDays(30));
    }

    [Fact]
    public async Task RedeliveredEvent_DoesNotExtendTwice()
    {
        var until = Now + Duration.FromDays(30);
        var payment = Payment(until, PaymentStatus.Paid);
        Event(PaymentWebhookOutcome.Paid);

        await Handler().Handle(new HandleStripeWebhookCommand("{}", "sig"), default);

        payment.User.PremiumUntil.Should().Be(until);
        unit.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExpiredEvent_MarksThePayment_WithoutGrantingPremium()
    {
        var payment = Payment();
        Event(PaymentWebhookOutcome.Expired);

        await Handler().Handle(new HandleStripeWebhookCommand("{}", "sig"), default);

        payment.Status.Should().Be(PaymentStatus.Expired);
        payment.User.PremiumUntil.Should().BeNull();
    }

    [Fact]
    public async Task UnknownSession_IsIgnored()
    {
        store.Setup(x => x.FindBySessionAsync("cs_1", It.IsAny<CancellationToken>())).ReturnsAsync((PaymentTransaction?)null);
        Event(PaymentWebhookOutcome.Paid);

        await Handler().Handle(new HandleStripeWebhookCommand("{}", "sig"), default);

        unit.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task InvalidSignature_StopsBeforeAnyChange()
    {
        gateway.Setup(x => x.ParseWebhook("{}", "bad")).Throws(new ValidationException("bad signature"));

        var act = () => Handler().Handle(new HandleStripeWebhookCommand("{}", "bad"), default);

        await act.Should().ThrowAsync<ValidationException>();
        store.Verify(x => x.FindBySessionAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(1, true)]
    [InlineData(3, true)]
    [InlineData(12, true)]
    [InlineData(0, false)]
    [InlineData(2, false)]
    public void CheckoutValidator_OnlyAcceptsTheOfferedPlans(int months, bool valid) =>
        new CreateCheckoutSessionCommandValidator().Validate(new CreateCheckoutSessionCommand(months)).IsValid.Should().Be(valid);

    [Fact]
    public void WebhookValidator_RequiresPayloadAndSignature() =>
        new HandleStripeWebhookCommandValidator().Validate(new HandleStripeWebhookCommand("", "")).Errors.Should().HaveCount(2);

    private static AdvancedMatchRow Row(string home, int homeScore, string away, int awayScore) =>
        new(Now, home, home.ToUpperInvariant(), homeScore, away, away.ToUpperInvariant(), awayScore);

    [Fact]
    public void TeamForm_CountsResultsFromTheTeamsSide()
    {
        var form = GetTeamFormQueryHandler.BuildForm("a", [Row("a", 2, "b", 0), Row("c", 1, "a", 1), Row("d", 3, "a", 1)]);

        form.SelectionName.Should().Be("A");
        (form.Wins, form.Draws, form.Losses, form.Points).Should().Be((1, 1, 1, 4));
        (form.GoalsFor, form.GoalsAgainst).Should().Be((4, 4));
        form.Matches.Select(x => x.Result).Should().Equal("W", "D", "L");
    }

    [Fact]
    public void HeadToHead_AttributesGoalsPerTeam_RegardlessOfVenue()
    {
        var result = GetHeadToHeadQueryHandler.BuildHeadToHead("a", [Row("a", 2, "b", 0), Row("b", 3, "a", 1), Row("b", 1, "a", 1)]);

        (result.FirstName, result.SecondName).Should().Be(("A", "B"));
        (result.FirstWins, result.Draws, result.SecondWins).Should().Be((1, 1, 1));
        (result.FirstGoals, result.SecondGoals).Should().Be((4, 4));
    }

    [Fact]
    public void EmptyHistory_GivesEmptyResults()
    {
        GetTeamFormQueryHandler.BuildForm("a", []).Matches.Should().BeEmpty();
        GetHeadToHeadQueryHandler.BuildHeadToHead("a", []).Matches.Should().BeEmpty();
    }
}
