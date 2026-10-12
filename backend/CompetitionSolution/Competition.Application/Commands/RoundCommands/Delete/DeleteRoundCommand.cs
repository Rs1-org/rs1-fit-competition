using MediatR;

namespace Competition.Application.Commands.RoundCommands.Delete;

public class DeleteRoundCommand : IRequest<Guid>
{
    public Guid Id { get; set; }
}
