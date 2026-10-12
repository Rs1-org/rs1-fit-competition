using Competition.Domain.Entities;

namespace Competition.Application.Commands.ClubCommands;

public abstract class ClubInput
{
    public string Name { get; set; } = string.Empty;
    public string FoundingDate { get; set; } = string.Empty;
    public ClubAge ClubAge { get; set; }
    public string CityId { get; set; } = string.Empty;
    public string StadiumId { get; set; } = string.Empty;
}
