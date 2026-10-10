namespace Competition.Application.DTOs.SelectionDto;

public class PlayerAppearanceDto
{
    public Guid MatchId { get; set; }
    public string? MatchSlug { get; set; }
    public string MatchDate { get; set; } = string.Empty;
    public string? StadiumName { get; set; }
    public Guid SelectionId { get; set; }
    public string SelectionName { get; set; } = string.Empty;
    public string Side { get; set; } = string.Empty;
    public int? ShirtNumber { get; set; }
}
