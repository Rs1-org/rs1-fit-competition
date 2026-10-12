using NodaTime;

namespace Competition.Domain.Entities.CompetitionEntities;

public class Season : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public LocalDate StartDate { get; set; }
    public LocalDate EndDate { get; set; }
    public bool IsCurrent { get; set; }

    public Guid CompetitionId { get; set; }
    public Competition? Competition { get; set; }

    public List<Round> Rounds { get; set; } = [];
}
