namespace Competition.Application.DTOs.SelectionDto;

public class PlayerAppearanceDto
{
    public string MatchPublicId { get; set; } = string.Empty;
    public string? MatchSlug { get; set; }
    public string MatchDate { get; set; } = string.Empty;
    public string? StadiumName { get; set; }
    public string SelectionPublicId { get; set; } = string.Empty;
    public string SelectionName { get; set; } = string.Empty;
    public string Side { get; set; } = string.Empty;
    public int? ShirtNumber { get; set; }
}
