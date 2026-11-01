namespace Competition.Application.DTOs.HistoryDto;

public class TransferValueHistoryDto
{
    public string PublicId { get; set; } = string.Empty;
    public string? Date { get; set; }
    public decimal? Value { get; set; }
    public string PlayerPublicId { get; set; } = string.Empty;
}
