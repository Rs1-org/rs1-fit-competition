using Competition.Application.DTOs.CityDto;
using Competition.Application.DTOs.StadiumDto;
using Competition.Domain.Entities;

namespace Competition.Application.DTOs.ClubDto;

public class ClubResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public ClubAge ClubAge { get; set; }
    public string? FoundingDate { get; set; }
    public Guid? CityId { get; set; }
    public string? CityName { get; set; }
    public Guid? StadiumId { get; set; }
    public string? StadiumName { get; set; }
    public CityDetailsDto? City { get; set; }
    public StadiumDetailsDto? Stadium { get; set; }
}
