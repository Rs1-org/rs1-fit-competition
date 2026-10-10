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
    public Guid StatusId { get; set; }
    public Guid StadiumId { get; set; }
    public Guid? HomeSelectionId { get; set; }
    public Guid? AwaySelectionId { get; set; }
}
