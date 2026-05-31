namespace Competition.Application.DTOs.MatchDto;

public class MatchDetailsDto
{
    public Guid Id { get; set; }
    public string? Slug { get; set; }
    public string? Date { get; set; }
    public int FollowersCount { get; set; }
    public int? HomeScore { get; set; }
    public int? AwayScore { get; set; }
    public int? HalfHomeScore { get; set; }
    public int? HalfAwayScore { get; set; }
    public bool IsPostponed { get; set; }
    public Guid StatusId { get; set; }
    public string? StatusName { get; set; }
    public Guid StadiumId { get; set; }
    public string? StadiumName { get; set; }
}
