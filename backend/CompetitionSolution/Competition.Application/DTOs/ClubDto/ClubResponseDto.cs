using Competition.Domain.Entities;
using NodaTime.Text;

namespace Competition.Application.DTOs.ClubDto;

public class ClubResponseDto
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string FoundingDate { get; init; } = string.Empty;
    public ClubAge ClubAge { get; init; }
    public string CityId { get; init; } = string.Empty;
    public string CityName { get; init; } = string.Empty;
    public string StadiumId { get; init; } = string.Empty;
    public string StadiumName { get; init; } = string.Empty;

    public static ClubResponseDto FromEntity(Club club) => new()
    {
        Id = club.PublicId, Name = club.Name, Slug = club.Slug,
        FoundingDate = LocalDatePattern.Iso.Format(club.FoundingDate), ClubAge = club.ClubAge,
        CityId = club.City.PublicId, CityName = club.City.Name ?? string.Empty,
        StadiumId = club.Stadium.PublicId, StadiumName = club.Stadium.Name ?? string.Empty
    };
}
