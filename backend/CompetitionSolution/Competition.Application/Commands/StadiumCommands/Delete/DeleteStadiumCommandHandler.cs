using Competition.Application.Interfaces;
using MediatR;

namespace Competition.Application.Commands.StadiumCommands.Delete;

public class DeleteStadiumCommandHandler : IRequestHandler<DeleteStadiumCommand, bool>
{
    private readonly IStadium _stadium;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteStadiumCommandHandler(IStadium stadium, IUnitOfWork unitOfWork)
    {
        _stadium = stadium;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeleteStadiumCommand request, CancellationToken cancellationToken)
    {
        var stadium = await _stadium.GetByIdAsync(request.Id);
        if (stadium == null)
        {
            throw new KeyNotFoundException($"Couldn't find any stadium with ID {request.Id}");
        }

        await _stadium.DeleteAsync(stadium);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }
}
