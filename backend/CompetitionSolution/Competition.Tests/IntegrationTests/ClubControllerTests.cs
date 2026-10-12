using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Competition.Application.Common.Models;
using Competition.Application.DTOs.ClubDto;
using Competition.Domain.Entities;
using Competition.Domain.Entities.MatchEntities;
using Competition.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Competition.Tests.IntegrationTests;

public class ClubControllerTests : IDisposable
{
    private readonly WebApplicationFactory<Program> factory;
    private readonly HttpClient client;
    private readonly string cityId;
    private readonly string stadiumId;
    private readonly string otherCityId;

    public ClubControllerTests()
    {
        var dbName = Guid.NewGuid().ToString();
        factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=unused",
                ["Jwt:Key"] = "ClubIntegrationTestsSecretAtLeast32Characters",
                ["Jwt:Issuer"] = "ClubTests", ["Jwt:Audience"] = "ClubTests",
                ["Cors:AllowedOrigins:0"] = "http://localhost:4200"
            }));
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<DbContextOptions<AppDbContext>>();
                services.RemoveAll<IDbContextOptionsConfiguration<AppDbContext>>();
                services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(dbName));
                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = "ClubTest";
                    options.DefaultChallengeScheme = "ClubTest";
                    options.DefaultForbidScheme = "ClubTest";
                }).AddScheme<AuthenticationSchemeOptions, ClubTestAuthenticationHandler>("ClubTest", _ => { });
            });
        });
        client = factory.CreateClient();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var city = new City { Id = Guid.NewGuid(), Name = "Sarajevo", Slug = "sarajevo", CountryId = Guid.Empty, GeoDataId = Guid.Empty };
        var other = new City { Id = Guid.NewGuid(), Name = "Mostar", Slug = "mostar", CountryId = Guid.Empty, GeoDataId = Guid.Empty };
        var stadium = new Stadium { Id = Guid.NewGuid(), Name = "Club test stadium", Slug = "club-stadium", City = city };
        db.AddRange(city, other, stadium); db.SaveChanges();
        cityId = city.PublicId; stadiumId = stadium.PublicId; otherCityId = other.PublicId;
    }

    private object Input(string name = "Test Club", string? city = null, string date = "1950-01-01") =>
        new { name, foundingDate = date, clubAge = 7, cityId = city ?? cityId, stadiumId };

    [Fact]
    public async Task Admin_CanCreateUpdateAndSoftDeleteClub()
    {
        client.DefaultRequestHeaders.Add("X-Club-Test-Role", "Admin");
        var create = await client.PostAsJsonAsync("/api/club", Input());
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        Assert.NotNull(create.Headers.Location);
        var club = (await create.Content.ReadFromJsonAsync<ClubResponseDto>())!;
        Assert.Equal(32, club.Id.Length);
        var filtered = await client.GetFromJsonAsync<PagedResult<ClubResponseDto>>($"/api/club?name=test&cityId={cityId}&stadiumId={stadiumId}&foundingYear=1950&clubAge=7&pageSize=1");
        Assert.Single(filtered!.Items); Assert.Equal(1, filtered.TotalCount);
        var update = await client.PutAsJsonAsync($"/api/club/{club.Slug}", Input("Renamed Club"));
        Assert.Equal(HttpStatusCode.OK, update.StatusCode);
        var renamed = (await update.Content.ReadFromJsonAsync<ClubResponseDto>())!;
        Assert.Equal("Renamed Club", renamed.Name);
        Assert.Equal(HttpStatusCode.OK, (await client.DeleteAsync($"/api/club/{club.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"/api/club/{renamed.Slug}")).StatusCode);
        using var scope = factory.Services.CreateScope();
        var row = await scope.ServiceProvider.GetRequiredService<AppDbContext>().Clubs.IgnoreQueryFilters().SingleAsync();
        Assert.NotNull(row.DeletedAt);
    }

    [Fact]
    public async Task Moderator_CanWriteButCannotDelete()
    {
        client.DefaultRequestHeaders.Add("X-Club-Test-Role", "Moderator");
        var response = await client.PostAsJsonAsync("/api/club", Input());
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var club = (await response.Content.ReadFromJsonAsync<ClubResponseDto>())!;
        Assert.Equal(HttpStatusCode.OK, (await client.PutAsJsonAsync($"/api/club/{club.Slug}", Input("Moderator Club"))).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.DeleteAsync($"/api/club/{club.Id}")).StatusCode);
    }

    [Theory]
    [InlineData(null, HttpStatusCode.Unauthorized)]
    [InlineData("User", HttpStatusCode.Forbidden)]
    [InlineData("Manager", HttpStatusCode.Forbidden)]
    public async Task NonManagers_CannotWrite(string? role, HttpStatusCode expected)
    {
        if (role != null) client.DefaultRequestHeaders.Add("X-Club-Test-Role", role);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/club")).StatusCode);
        Assert.Equal(expected, (await client.PostAsJsonAsync("/api/club", Input())).StatusCode);
        Assert.Equal(expected, (await client.PutAsJsonAsync("/api/club/unknown", Input())).StatusCode);
        Assert.Equal(expected, (await client.DeleteAsync($"/api/club/{BaseEntity.NewPublicId()}")).StatusCode);
    }

    [Fact]
    public async Task InvalidDateMismatchedCityAndDuplicateName_AreRejected()
    {
        client.DefaultRequestHeaders.Add("X-Club-Test-Role", "Admin");
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync("/api/club", Input(date: "2050-01-01"))).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync("/api/club", Input(city: otherCityId))).StatusCode);
        Assert.Equal(HttpStatusCode.Created, (await client.PostAsJsonAsync("/api/club", Input())).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync("/api/club", Input())).StatusCode);
    }

    [Theory]
    [InlineData("page=0")]
    [InlineData("pageSize=101")]
    [InlineData("foundingYear=0")]
    [InlineData("clubAge=99")]
    public async Task InvalidFilters_ReturnBadRequest(string query) =>
        Assert.Equal(HttpStatusCode.BadRequest, (await client.GetAsync($"/api/club?{query}")).StatusCode);

    public void Dispose() { client.Dispose(); factory.Dispose(); }
}

public class ClubTestAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("X-Club-Test-Role", out var role)) return Task.FromResult(AuthenticateResult.NoResult());
        var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "club-test"), new Claim(ClaimTypes.Role, role.ToString()) }, Scheme.Name);
        return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name)));
    }
}
