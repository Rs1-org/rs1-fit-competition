using MediatR;

namespace Competition.Application.Commands.ClubCommands.Delete;

public class DeleteClubCommand : IRequest<bool>
{
    public Guid Id { get; set; }
}
