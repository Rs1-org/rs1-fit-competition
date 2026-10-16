using Competition.Application.DTOs.MatchDto;
using MediatR;

namespace Competition.Application.Commands.MatchCommands.Create;

public class CreateMatchCommand : IRequest<MatchDetailsDto>
{
    public DateTime Date { get; set; }
    public int FollowersCount { get; set; }
    public bool IsPostponed { get; set; }
    public string? Slug { get; set; }
    public string StatusPublicId { get; set; } = string.Empty;
    public string StadiumPublicId { get; set; } = string.Empty;
    public string? HomeSelectionPublicId { get; set; }
    public string? AwaySelectionPublicId { get; set; }
    public string? RoundPublicId { get; set; }
}
