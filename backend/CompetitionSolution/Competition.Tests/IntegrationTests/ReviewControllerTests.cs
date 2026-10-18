using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Competition.Application.Auth.Interfaces;
using Competition.Application.Reviews;
using Competition.Domain.Entities;
using Competition.Domain.Entities.MatchEntities;
using Competition.Infrastructure.Data;
using Competition.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Competition.Tests.IntegrationTests;

public class ReviewControllerTests(WebApplicationFactory<Program> factory) : BaseIntegrationTest(factory)
{
    private async Task<Match> Match()
    {
        using var scope = ServiceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var match = StatisticTestData.NewMatch(); db.Matches.Add(match); await db.SaveChangesAsync(); return match;
    }
    private async Task<ApplicationUser> User()
    {
        using var scope = ServiceProvider.CreateScope();
        var manager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var email = $"review-{Guid.NewGuid():N}@example.invalid";
        var user = new ApplicationUser { Email = email, UserName = email, FirstName = "Reviewer", LastName = "Test" };
        (await manager.CreateAsync(user)).Succeeded.Should().BeTrue();
        await manager.AddToRoleAsync(user, "User"); Authenticate(user); return user;
    }
    private void Authenticate(ApplicationUser user)
    {
        using var scope = ServiceProvider.CreateScope();
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
            scope.ServiceProvider.GetRequiredService<IJwtTokenGenerator>().GenerateJwtToken(user, ["User"]));
    }
    private async Task<SavedReviewDto> Create(string match, int rating)
    {
        var response = await Client.PostAsJsonAsync($"/api/reviews/match/{match}", new { rating, comment = "A considered match review." });
        response.StatusCode.Should().Be(HttpStatusCode.OK); return (await response.Content.ReadFromJsonAsync<SavedReviewDto>())!;
    }

    [Fact]
    public async Task RatingCrud_IsOwnedUniqueAndSoftDeleted_WithCorrectAverageAndPaging()
    {
        var match = await Match(); var first = await User(); var review = await Create(match.PublicId, 4);
        (await Client.PostAsJsonAsync($"/api/reviews/match/{match.PublicId}", new { rating = 3, comment = "Duplicate" })).StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await Client.PutAsJsonAsync($"/api/reviews/{review.PublicId}", new { rating = 5, comment = "Updated review" })).StatusCode.Should().Be(HttpStatusCode.OK);
        await User(); await Create(match.PublicId, 3);
        (await Client.PutAsJsonAsync($"/api/reviews/{review.PublicId}", new { rating = 1, comment = "Not mine" })).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await Client.DeleteAsync($"/api/reviews/{review.PublicId}")).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        Authenticate(first);
        var page = (await Client.GetFromJsonAsync<MatchReviewPage>($"/api/reviews/match/{match.PublicId}?pageSize=1&page=2"))!;
        page.ReviewCount.Should().Be(2); page.AverageRating.Should().Be(4); page.Reviews.Items.Should().ContainSingle();
        page.MyReview!.PublicId.Should().Be(review.PublicId); page.MyReview.IsMine.Should().BeTrue();
        var json = await Client.GetStringAsync($"/api/reviews/match/{match.PublicId}");
        json.Should().NotContain(first.Id.ToString()).And.NotContain(first.Email!).And.NotContain("authorId");
        (await Client.DeleteAsync($"/api/reviews/{review.PublicId}")).StatusCode.Should().Be(HttpStatusCode.OK);
        var after = (await Client.GetFromJsonAsync<MatchReviewPage>($"/api/reviews/match/{match.PublicId}"))!;
        after.AverageRating.Should().Be(3); after.MyReview.Should().BeNull();
        await Create(match.PublicId, 2);
        using var scope = ServiceProvider.CreateScope();
        (await scope.ServiceProvider.GetRequiredService<AppDbContext>().MatchReviews.IgnoreQueryFilters().CountAsync()).Should().Be(3);
    }

    [Fact]
    public async Task Reactions_ToggleAndSwitch_AndCannotTargetOwnReview()
    {
        var match = await Match(); await User(); var review = await Create(match.PublicId, 4);
        (await Client.PutAsJsonAsync($"/api/reviews/{review.PublicId}/reaction", new { value = 1 })).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        await User();
        async Task<MatchReviewPage> React(int value)
        {
            (await Client.PutAsJsonAsync($"/api/reviews/{review.PublicId}/reaction", new { value })).StatusCode.Should().Be(HttpStatusCode.OK);
            return (await Client.GetFromJsonAsync<MatchReviewPage>($"/api/reviews/match/{match.PublicId}"))!;
        }
        var liked = (await React(1)).Reviews.Items.Single(); liked.LikeCount.Should().Be(1); liked.MyReaction.Should().Be(1);
        var disliked = (await React(-1)).Reviews.Items.Single(); disliked.LikeCount.Should().Be(0); disliked.DislikeCount.Should().Be(1);
        var removed = (await React(-1)).Reviews.Items.Single(); removed.MyReaction.Should().BeNull(); removed.DislikeCount.Should().Be(0);
        (await Client.PutAsJsonAsync($"/api/reviews/{review.PublicId}/reaction", new { value = 2 })).StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData(0, "Comment")]
    [InlineData(6, "Comment")]
    [InlineData(4, "   ")]
    public async Task InvalidRatingsOrComments_AreRejected(int rating, string comment)
    {
        var match = await Match(); await User();
        (await Client.PostAsJsonAsync($"/api/reviews/match/{match.PublicId}", new { rating, comment })).StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AnonymousReads_ArePagedAndEmptyAverageIsNull_AndWritesRequireLogin()
    {
        var match = await Match();
        var page = (await Client.GetFromJsonAsync<MatchReviewPage>($"/api/reviews/match/{match.PublicId}"))!;
        page.AverageRating.Should().BeNull(); page.ReviewCount.Should().Be(0); page.MyReview.Should().BeNull();
        (await Client.PostAsJsonAsync($"/api/reviews/match/{match.PublicId}", new { rating = 4, comment = "Comment" })).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        (await Client.GetAsync($"/api/reviews/match/{match.PublicId}?page=0")).StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
