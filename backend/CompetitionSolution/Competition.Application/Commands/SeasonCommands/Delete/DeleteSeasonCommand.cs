using MediatR;

namespace Competition.Application.Commands.SeasonCommands.Delete;

public class DeleteSeasonCommand : IRequest<Guid>
{
    public Guid Id { get; set; }
}
