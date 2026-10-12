namespace Competition.Application.DTOs.SeasonDto;

public class SeasonDetailsDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string StartDate { get; set; } = string.Empty;
    public string EndDate { get; set; } = string.Empty;
    public bool IsCurrent { get; set; }
    public Guid CompetitionId { get; set; }
    public string CompetitionName { get; set; } = string.Empty;
    public string CompetitionSlug { get; set; } = string.Empty;
}
