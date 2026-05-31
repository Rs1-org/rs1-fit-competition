using MediatR;

namespace Competition.Application.Commands.MatchCommands.Delete;

public class DeleteMatchCommand : IRequest<Guid>
{
    public Guid Id { get; set; }
}
