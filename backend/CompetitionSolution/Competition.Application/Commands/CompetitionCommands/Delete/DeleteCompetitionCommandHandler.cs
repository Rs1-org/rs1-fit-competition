using Competition.Application.Helpers;
using Competition.Application.Interfaces;
using MediatR;

namespace Competition.Application.Commands.CompetitionCommands.Delete;

public class DeleteCompetitionCommandHandler : IRequestHandler<DeleteCompetitionCommand, Guid>
{
    private readonly ICompetition _competition;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCompetitionCommandHandler(ICompetition competition, IUnitOfWork unitOfWork)
    {
        _competition = competition;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(DeleteCompetitionCommand request, CancellationToken cancellationToken)
    {
        var competition = await _competition.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Competition with ID '{request.Id}' not found.");

        // Block instead of cascading so no season or round is left without a parent.
        if (await _competition.HasSeasonsAsync(competition.Id, cancellationToken))
            throw BusinessRule.Violation(nameof(request.Id), "This competition still has seasons. Delete its seasons first.");

        await _competition.DeleteAsync(competition, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return request.Id;
    }
}
