using MediatR;

namespace Competition.Application.Commands.RefereeCommands.Delete;

public class DeleteRefereeCommand(Guid id) : IRequest<Guid>
{
    public Guid Id { get; } = id;
}
