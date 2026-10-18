namespace Competition.Domain.Entities.MatchEntities;

public class TeamStatistic : BaseEntity
{
    public Guid StatisticId { get; set; }
    public Statistic Statistic { get; set; } = null!;
    public Guid SelectionId { get; set; }
    public Selection Selection { get; set; } = null!;
    public int TotalShots { get; set; }
    public int ShotsOnTarget { get; set; }
    public int TotalRedCards { get; set; }
    public int TotalYellowCards { get; set; }
    public int Corners { get; set; }
    public int BigChances { get; set; }
    public int Possession { get; set; }
}
