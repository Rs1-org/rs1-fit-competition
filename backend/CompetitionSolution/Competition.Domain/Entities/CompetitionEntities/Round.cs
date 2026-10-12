using Competition.Domain.Entities.MatchEntities;

namespace Competition.Domain.Entities.CompetitionEntities;

public class Round : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public int Number { get; set; }

    public Guid SeasonId { get; set; }
    public Season? Season { get; set; }

    public List<Match> Matches { get; set; } = [];
}
