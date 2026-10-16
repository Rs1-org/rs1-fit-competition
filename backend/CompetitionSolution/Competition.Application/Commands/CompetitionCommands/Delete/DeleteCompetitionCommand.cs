using MediatR;

namespace Competition.Application.Commands.CompetitionCommands.Delete;

public class DeleteCompetitionCommand : IRequest<string>
{
    public string PublicId { get; set; } = string.Empty;
}
