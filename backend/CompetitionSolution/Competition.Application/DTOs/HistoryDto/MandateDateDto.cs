namespace Competition.Application.DTOs.HistoryDto;

public class MandateDateDto
{
    public Guid Id { get; set; }
    public string? StartDate { get; set; }
    public string? EndDate { get; set; }
    public Guid? PlayerSelectionHistoryId { get; set; }
    public Guid? SelectionManagerHistoryId { get; set; }
}
