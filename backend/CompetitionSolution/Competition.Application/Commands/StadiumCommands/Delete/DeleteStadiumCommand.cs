using MediatR;

namespace Competition.Application.Commands.StadiumCommands.Delete;

public class DeleteStadiumCommand : IRequest<bool>
{
    public string PublicId { get; set; } = string.Empty;
}
