namespace Competition.Application.DTOs.HistoryDto;

public class TransferValueHistoryDto
{
    public Guid Id { get; set; }
    public string? Date { get; set; }
    public decimal? Value { get; set; }
    public Guid PlayerId { get; set; }
}
