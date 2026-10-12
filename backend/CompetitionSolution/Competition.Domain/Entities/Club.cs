using Competition.Domain.Entities.MatchEntities;
using NodaTime;

namespace Competition.Domain.Entities;

public enum ClubAge { U7, U9, U11, U13, U16, U19, U21, Senior }

public class Club : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public LocalDate FoundingDate { get; set; }
    public ClubAge ClubAge { get; set; }
    public Guid CityId { get; set; }
    public virtual City City { get; set; } = null!;
    public Guid StadiumId { get; set; }
    public virtual Stadium Stadium { get; set; } = null!;
}
