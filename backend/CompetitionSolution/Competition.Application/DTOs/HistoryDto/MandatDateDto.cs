namespace Competition.Application.DTOs.HistoryDto;

public class MandatDateDto
{
    public Guid Id { get; set; }
    public string? StartMandatDate { get; set; }
    public string? EndMandatDate { get; set; }
    public Guid? PlayerSelectionHistoryId { get; set; }
    public Guid? SelectionManagerHistoryId { get; set; }
}
