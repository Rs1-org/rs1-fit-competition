using Competition.Domain.Entities;
using Competition.Domain.Entities.MatchEntities;
using NodaTime;

namespace Competition.Tests.Infrastructure;

public static class StatisticTestData
{
    public static Match NewMatch()
    {
        var suffix = Guid.NewGuid().ToString("N");
        var country = new Country { Name = "Statistics test", Slug = $"stats-country-{suffix}" };
        var geo = new GeoData();
        var city = new City
        {
            Name = "Statistics city", Slug = $"stats-city-{suffix}", Country = country, GeoData = geo,
            CountryId = Guid.Empty, GeoDataId = Guid.Empty,
        };
        return new Match
        {
            Slug = $"stats-match-{suffix}", Date = SystemClock.Instance.GetCurrentInstant(),
            Status = new MatchStatus { Name = "Scheduled", Slug = $"stats-status-{suffix}" },
            Stadium = new Stadium
            {
                Name = "Statistics stadium", Slug = $"stats-stadium-{suffix}", City = city, GeoData = geo,
                DateOfConstruction = DateTime.UtcNow, Capacity = 100,
            },
            HomeSelection = new Selection { Name = "Home", Abbreviation = "HOM", Country = country, Slug = $"stats-home-{suffix}" },
            AwaySelection = new Selection { Name = "Away", Abbreviation = "AWY", Country = country, Slug = $"stats-away-{suffix}" },
        };
    }
}
