namespace Competition.Application.DTOs.HistoryDto;

public class PlayerSelectionHistoryDto
{
    public Guid Id { get; set; }
    public int? Goals { get; set; }
    public int? MatchPlayed { get; set; }
    public int? Assists { get; set; }
    public int? ShirtNumber { get; set; }
    public Guid PlayerId { get; set; }
}
