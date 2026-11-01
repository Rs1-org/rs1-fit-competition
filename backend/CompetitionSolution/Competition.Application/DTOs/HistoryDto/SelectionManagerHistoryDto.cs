namespace Competition.Application.DTOs.HistoryDto;

public class SelectionManagerHistoryDto
{
    public string PublicId { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public int? Wins { get; set; }
    public int? Loses { get; set; }
    public int? Draw { get; set; }
    public string ManagerPublicId { get; set; } = string.Empty;
    public string? SelectionPublicId { get; set; }
}
