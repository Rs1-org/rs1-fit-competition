using Competition.Domain.Entities.Person;

namespace Competition.Domain.Entities.FormationEntities;

public class Position : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Slug { get; set; }

    public ICollection<SubPosition> SubPositions { get; set; } = new List<SubPosition>();
    public ICollection<Player> Players { get; set; } = new List<Player>();
}
