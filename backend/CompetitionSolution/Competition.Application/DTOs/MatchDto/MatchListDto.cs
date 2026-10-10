namespace Competition.Application.DTOs.MatchDto;

public class MatchListDto
{
    public Guid Id { get; set; }
    public string? Slug { get; set; }
    public string? Date { get; set; }
    public int? HomeScore { get; set; }
    public int? AwayScore { get; set; }
    public bool IsPostponed { get; set; }
    public string? StatusName { get; set; }
    public string? StadiumName { get; set; }
    public string? HomeSelectionName { get; set; }
    public string? AwaySelectionName { get; set; }
}
