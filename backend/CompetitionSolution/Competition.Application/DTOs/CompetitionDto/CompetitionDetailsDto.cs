namespace Competition.Application.DTOs.CompetitionDto;

public class CompetitionDetailsDto
{
    public string PublicId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? ShortName { get; set; }
    public string? Description { get; set; }
    public string? CountryPublicId { get; set; }
    public string? CountryName { get; set; }
}
