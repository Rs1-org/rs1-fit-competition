namespace Competition.Domain.Entities.MatchEntities;

public class Statistic : BaseEntity
{
    public Guid MatchId { get; set; }
    public Match Match { get; set; } = null!;
    public ICollection<TeamStatistic> TeamStatistics { get; set; } = [];
}
