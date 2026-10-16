using Competition.Application.Helpers;
using Competition.Application.Interfaces;
using CompetitionEntity = Competition.Domain.Entities.CompetitionEntities.Competition;
using MediatR;

namespace Competition.Application.Commands.CompetitionCommands.Delete;

public class DeleteCompetitionCommandHandler : IRequestHandler<DeleteCompetitionCommand, string>
{
    private readonly ICompetition _competition;
    private readonly IPublicIdLookup _lookup;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCompetitionCommandHandler(ICompetition competition, IPublicIdLookup lookup, IUnitOfWork unitOfWork)
    {
        _lookup = lookup;
        _competition = competition;
        _unitOfWork = unitOfWork;
    }

    public async Task<string> Handle(DeleteCompetitionCommand request, CancellationToken cancellationToken)
    {
        var competition = await _lookup.GetRequiredEntityAsync<CompetitionEntity>(request.PublicId, cancellationToken);

        // Block instead of cascading so no season or round is left without a parent.
        if (await _competition.HasSeasonsAsync(competition.Id, cancellationToken))
            throw BusinessRule.Violation(nameof(request.PublicId), "This competition still has seasons. Delete its seasons first.");

        await _competition.DeleteAsync(competition, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return request.PublicId;
    }
}
