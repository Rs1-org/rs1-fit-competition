using Competition.Domain.Entities.Person;

namespace Competition.Domain.Entities.FormationEntities;

public class SubPosition : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Slug { get; set; }

    public Guid PositionId { get; set; }
    public Position? Position { get; set; }

    public ICollection<Player> Players { get; set; } = new List<Player>();
}
