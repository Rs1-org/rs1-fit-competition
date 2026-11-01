namespace Competition.Application.DTOs.HistoryDto;

public class MandateDateDto
{
    public string PublicId { get; set; } = string.Empty;
    public string? StartDate { get; set; }
    public string? EndDate { get; set; }
    public string? PlayerSelectionHistoryPublicId { get; set; }
    public string? SelectionManagerHistoryPublicId { get; set; }
}
