namespace Competition.Application.DTOs.HistoryDto;

public class SelectionManagerHistoryDto
{
    public Guid Id { get; set; }
    public string? Notes { get; set; }
    public int? Wins { get; set; }
    public int? Loses { get; set; }
    public int? Draw { get; set; }
    public Guid ManagerId { get; set; }
}
