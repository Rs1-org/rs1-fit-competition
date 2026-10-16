using MediatR;

namespace Competition.Application.Commands.SeasonCommands.Delete;

public class DeleteSeasonCommand : IRequest<string>
{
    public string PublicId { get; set; } = string.Empty;
}
