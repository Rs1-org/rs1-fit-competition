using MediatR;

namespace Competition.Application.Commands.RoundCommands.Delete;

public class DeleteRoundCommand : IRequest<string>
{
    public string PublicId { get; set; } = string.Empty;
}
