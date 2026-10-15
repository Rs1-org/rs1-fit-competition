using Competition.Application.Interfaces;
using Competition.Application.Queries.SelectionQueries.GetAll;
using Competition.Domain.Entities;
using Competition.Infrastructure.Data;
using Competition.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Competition.Tests.UnitTests.SelectionPublicId;

public class SelectionListFilterTests
{
    private static readonly SelectionListFilter NoFilter = new(null, null, null, null, null);

    private static AppDbContext NewContext() =>
        new(new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase("Selection_" + Guid.NewGuid()).Options);

    [Fact]
    public async Task Filters_NameCountryAbbreviationAndFollowers_ShouldApplyTogether()
    {
        await using var context = NewContext();
        var bosnia = new Country { Id = Guid.NewGuid(), Name = "Bosnia and Herzegovina", Slug = "bosnia", PublicId = new string('a', 32) };
        var croatia = new Country { Id = Guid.NewGuid(), Name = "Croatia", Slug = "croatia", PublicId = new string('b', 32) };
        context.Countries.AddRange(bosnia, croatia);
        await context.SaveChangesAsync();

        context.Selections.AddRange(
            NewSelection("Bosnia and Herzegovina", "BIH", 1_200_000, bosnia),
            NewSelection("Bosnia U21", "BHU", 40_000, bosnia),
            NewSelection("Croatia", "CRO", 900_000, croatia));
        await context.SaveChangesAsync();

        var repository = new SelectionRepository(context);

        var byName = await repository.GetPagedAsync(NoFilter with { Name = "u21" }, 1, 10);
        var byCountry = await repository.GetPagedAsync(NoFilter with { CountryPublicId = bosnia.PublicId }, 1, 10);
        var byAbbreviation = await repository.GetPagedAsync(NoFilter with { Abbreviation = "cro" }, 1, 10);
        var byFollowers = await repository.GetPagedAsync(NoFilter with { FollowersMin = 100_000, FollowersMax = 1_000_000 }, 1, 10);
        var combined = await repository.GetPagedAsync(
            NoFilter with { Name = "Bosnia", CountryPublicId = bosnia.PublicId, Abbreviation = "bih", FollowersMin = 1_000_000, FollowersMax = 2_000_000 },
            1,
            10);

        byName.Items.Should().ContainSingle(s => s.Abbreviation == "BHU");
        byCountry.Items.Should().HaveCount(2);
        byAbbreviation.Items.Should().ContainSingle(s => s.Abbreviation == "CRO");
        byFollowers.Items.Should().ContainSingle(s => s.Abbreviation == "CRO");
        combined.Items.Should().ContainSingle(s => s.Abbreviation == "BIH");
    }

    [Fact]
    public void Validator_FollowersMaxBelowMin_ShouldFail()
    {
        var result = new GetAllSelectionsQueryValidator().Validate(new GetAllSelectionsQuery
        {
            FollowersMin = 100,
            FollowersMax = 10
        });

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.ErrorMessage == "Followers max must be greater than or equal to followers min.");
    }

    private static Selection NewSelection(string name, string abbreviation, int followers, Country country) => new()
    {
        Name = name,
        Abbreviation = abbreviation,
        FollowersCount = followers,
        CountryId = country.Id,
        Country = country,
        Slug = abbreviation.ToLowerInvariant()
    };
}
