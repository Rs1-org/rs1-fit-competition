namespace Competition.Application.DTOs.SelectionDto;

public class SelectionDto
{
    public Guid Id { get; set; }
    public string PublicId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Abbreviation { get; set; } = string.Empty;
    public string? PictureUrl { get; set; }
    public int FollowersCount { get; set; }
    public string? Slug { get; set; }
    public string? ClubPublicId { get; set; }
    public string? ClubName { get; set; }
    public string? ClubSlug { get; set; }
    public string? CountryPublicId { get; set; }
    public string? CountryName { get; set; }
    public string? HomeStadiumPublicId { get; set; }
    public string? StadiumName { get; set; }
    public string? StadiumSlug { get; set; }
}
