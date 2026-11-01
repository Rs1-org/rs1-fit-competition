namespace Competition.Application.DTOs.HistoryDto;

public class PlayerSelectionHistoryDto
{
    public string PublicId { get; set; } = string.Empty;
    public int? Goals { get; set; }
    public int? MatchPlayed { get; set; }
    public int? Assists { get; set; }
    public int? ShirtNumber { get; set; }
    public string PlayerPublicId { get; set; } = string.Empty;
    public string? SelectionPublicId { get; set; }
}
