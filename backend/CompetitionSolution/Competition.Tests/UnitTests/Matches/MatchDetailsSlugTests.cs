using AutoMapper;
using Competition.Application.DTOs.MatchDto;
using Competition.Application.Mappings;
using Competition.Domain.Entities.CompetitionEntities;
using Competition.Domain.Entities.MatchEntities;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using CompetitionEntity = Competition.Domain.Entities.CompetitionEntities.Competition;

namespace Competition.Tests.UnitTests.Matches;

/// <summary>COMP-128: match details carry the slugs the breadcrumb links to.</summary>
public class MatchDetailsSlugTests
{
    private static readonly IMapper Mapper = new ServiceCollection()
        .AddLogging()
        .AddAutoMapper((_, _) => { }, typeof(MappingProfile).Assembly)
        .BuildServiceProvider()
        .GetRequiredService<IMapper>();

    [Fact]
    public void Details_ShouldCarryCompetitionAndSeasonSlugs()
    {
        var match = new Match
        {
            Round = new Round
            {
                Name = "Round 1",
                Slug = "round-1",
                Season = new Season { Name = "2026/27", Slug = "2026-27", Competition = new CompetitionEntity { Name = "Premijer liga", Slug = "premijer-liga" } }
            }
        };

        var dto = Mapper.Map<MatchDetailsDto>(match);

        dto.SeasonSlug.Should().Be("2026-27");
        dto.CompetitionSlug.Should().Be("premijer-liga");
    }

    [Fact]
    public void Details_WithoutRound_ShouldLeaveTheSlugsEmpty()
    {
        var dto = Mapper.Map<MatchDetailsDto>(new Match());

        dto.SeasonSlug.Should().BeNull();
        dto.CompetitionSlug.Should().BeNull();
    }
}
