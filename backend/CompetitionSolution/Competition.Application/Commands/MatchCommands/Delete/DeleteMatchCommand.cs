using MediatR;

namespace Competition.Application.Commands.MatchCommands.Delete;

public class DeleteMatchCommand : IRequest<string>
{
    public string PublicId { get; set; } = string.Empty;
}
