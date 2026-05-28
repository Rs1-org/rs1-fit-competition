using MediatR;

namespace Competition.Application.Features.PlayerCommands.Delete;

public class DeletePlayerCommand(Guid id) : IRequest<Guid>
{
    public Guid Id { get; } = id;
}
