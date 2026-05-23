using Competition.Application.Interfaces;
using MediatR;

namespace Competition.Application.Commands.StadiumCommands.Delete;

public class DeleteStadiumCommandHandler(IStadium stadium, IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteStadiumCommand, bool>
{
    public async Task<bool> Handle(DeleteStadiumCommand request, CancellationToken cancellationToken)
    {
        var stadium1 = await stadium.GetByIdAsync(request.Id);
        if (stadium1 == null)
        {
            throw new KeyNotFoundException($"Couldn't find any stadium with ID {request.Id}");
        }

        await stadium.DeleteAsync(stadium1);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
