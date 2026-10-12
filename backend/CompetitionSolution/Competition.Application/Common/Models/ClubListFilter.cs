using Competition.Domain.Entities;

namespace Competition.Application.Common.Models;

public class ClubListFilter
{
    public string? Name { get; init; }
    public string? CityId { get; init; }
    public string? StadiumId { get; init; }
    public int? FoundingYear { get; init; }
    public ClubAge? ClubAge { get; init; }
}
