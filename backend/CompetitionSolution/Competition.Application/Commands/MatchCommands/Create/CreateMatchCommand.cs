using Competition.Application.DTOs.MatchDto;
using MediatR;

namespace Competition.Application.Commands.MatchCommands.Create;

public class CreateMatchCommand : IRequest<MatchDetailsDto>
{
    public DateTime Date { get; set; }
    public int FollowersCount { get; set; }
    public bool IsPostponed { get; set; }
    public string? Slug { get; set; }
    public Guid StatusId { get; set; }
    public Guid StadiumId { get; set; }
    public Guid? HomeSelectionId { get; set; }
    public Guid? AwaySelectionId { get; set; }
}
