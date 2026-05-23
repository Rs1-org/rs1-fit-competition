using Competition.Application.Interfaces;
using MediatR;

namespace Competition.Application.Commands.ClubCommands.Delete;

public class DeleteClubCommandHandler(IClubRepository clubRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteClubCommand, bool>
{
    public async Task<bool> Handle(DeleteClubCommand request, CancellationToken cancellationToken)
    {
        var club = await clubRepository.GetByIdAsync(request.Id);
        if (club == null)
        {
            throw new KeyNotFoundException($"Couldn't find any club with ID {request.Id}");
        }

        await clubRepository.DeleteAsync(club);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
