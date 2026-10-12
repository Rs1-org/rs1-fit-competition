using MediatR;

namespace Competition.Application.Commands.CompetitionCommands.Delete;

public class DeleteCompetitionCommand : IRequest<Guid>
{
    public Guid Id { get; set; }
}
