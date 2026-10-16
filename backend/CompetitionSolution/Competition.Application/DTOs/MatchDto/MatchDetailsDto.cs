namespace Competition.Application.DTOs.MatchDto;

public class MatchDetailsDto
{
    public string PublicId { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public string? Date { get; set; }
    public int FollowersCount { get; set; }
    public int? HomeScore { get; set; }
    public int? AwayScore { get; set; }
    public int? HalfHomeScore { get; set; }
    public int? HalfAwayScore { get; set; }
    public bool IsPostponed { get; set; }
    public string StatusPublicId { get; set; } = string.Empty;
    public string? StatusName { get; set; }
    public string StadiumPublicId { get; set; } = string.Empty;
    public string? StadiumName { get; set; }
    public string? HomeSelectionPublicId { get; set; }
    public string? HomeSelectionName { get; set; }
    public string? AwaySelectionPublicId { get; set; }
    public string? AwaySelectionName { get; set; }
    public string? RoundPublicId { get; set; }
    public string? RoundName { get; set; }
}
