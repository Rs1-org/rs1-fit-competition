using MediatR;

namespace Competition.Application.Commands.PlayerCommands.Delete;

public class DeletePlayerCommand(Guid id) : IRequest<Guid>
{
    public Guid Id { get; } = id;
}
