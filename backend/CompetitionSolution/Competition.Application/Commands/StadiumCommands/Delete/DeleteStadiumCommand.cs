using MediatR;

namespace Competition.Application.Commands.StadiumCommands.Delete;

public class DeleteStadiumCommand : IRequest<bool>
{
    public Guid Id { get; set; }
}
