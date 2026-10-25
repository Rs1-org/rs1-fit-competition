namespace Competition.Domain.Entities.MatchEntities;

public class MatchStatus : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public int SortOrder { get; set; }

    public ICollection<Match> Matches { get; set; } = new List<Match>();
}
