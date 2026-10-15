namespace Competition.Application.DTOs.SelectionDto;

public class SelectionDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Abbreviation { get; set; } = string.Empty;
    public string? PictureUrl { get; set; }
    public int FollowersCount { get; set; }
    public string? Slug { get; set; }
    public string? CountryPublicId { get; set; }
    public string? CountryName { get; set; }
}
