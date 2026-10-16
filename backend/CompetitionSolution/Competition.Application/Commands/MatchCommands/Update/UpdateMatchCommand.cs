using System.Text.Json.Serialization;
using Competition.Application.DTOs.MatchDto;
using MediatR;

namespace Competition.Application.Commands.MatchCommands.Update;

public class UpdateMatchCommand : IRequest<MatchDetailsDto>
{
    [JsonIgnore]
    public string Slug { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public int FollowersCount { get; set; }
    public int? HomeScore { get; set; }
    public int? AwayScore { get; set; }
    public int? HalfHomeScore { get; set; }
    public int? HalfAwayScore { get; set; }
    public bool IsPostponed { get; set; }
    public string StatusPublicId { get; set; } = string.Empty;
    public string StadiumPublicId { get; set; } = string.Empty;
    public string? HomeSelectionPublicId { get; set; }
    public string? AwaySelectionPublicId { get; set; }
    public string? RoundPublicId { get; set; }
}
