using MediatR;

namespace Competition.Application.Features.ManagerCommands.Delete;

public class DeleteManagerCommand(Guid id) : IRequest<Guid>
{
    public Guid Id { get; } = id;
}
