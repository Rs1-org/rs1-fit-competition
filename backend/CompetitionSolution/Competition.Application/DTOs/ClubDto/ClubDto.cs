namespace Competition.Application.DTOs.ClubDto;

public class ClubListDto
{
    public string PublicId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? FoundingDate { get; set; }
    public int ClubAge { get; set; }
    public string? CityName { get; set; }
    public string? CountryName { get; set; }
    public string? StadiumName { get; set; }
}

public class ClubDetailsDto : ClubListDto
{
    public string? CityPublicId { get; set; }
    public string? CountryPublicId { get; set; }
    public string? StadiumPublicId { get; set; }
    public string? StadiumSlug { get; set; }
}

public record DeletedClubDto(string PublicId);
