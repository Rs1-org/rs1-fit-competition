using Competition.Domain.Entities.MatchEntities;
using NodaTime;

namespace Competition.Domain.Entities;

public enum ClubAge { U7, U9, U11, U13, U16, U19, U21, Senior }

public class Club : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public LocalDate? FoundingDate { get; set; }
    public ClubAge ClubAge { get; set; } = ClubAge.Senior;
    public Guid? CityId { get; set; }
    public City? City { get; set; }
    public Guid? StadiumId { get; set; }
    public Stadium? Stadium { get; set; }
}
