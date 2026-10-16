namespace Competition.Application.DTOs.SeasonDto;

public class SeasonDetailsDto
{
    public string PublicId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string StartDate { get; set; } = string.Empty;
    public string EndDate { get; set; } = string.Empty;
    public bool IsCurrent { get; set; }
    public string CompetitionPublicId { get; set; } = string.Empty;
    public string CompetitionName { get; set; } = string.Empty;
    public string CompetitionSlug { get; set; } = string.Empty;
}
