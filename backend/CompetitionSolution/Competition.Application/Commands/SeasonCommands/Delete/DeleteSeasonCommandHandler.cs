using Competition.Application.Helpers;
using Competition.Application.Interfaces;
using Competition.Domain.Entities.CompetitionEntities;
using MediatR;

namespace Competition.Application.Commands.SeasonCommands.Delete;

public class DeleteSeasonCommandHandler : IRequestHandler<DeleteSeasonCommand, string>
{
    private readonly ISeason _season;
    private readonly IPublicIdLookup _lookup;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteSeasonCommandHandler(ISeason season, IPublicIdLookup lookup, IUnitOfWork unitOfWork)
    {
        _lookup = lookup;
        _season = season;
        _unitOfWork = unitOfWork;
    }

    public async Task<string> Handle(DeleteSeasonCommand request, CancellationToken cancellationToken)
    {
        var season = await _lookup.GetRequiredEntityAsync<Season>(request.PublicId, cancellationToken);

        // Block instead of cascading so no round is left without a parent.
        if (await _season.HasRoundsAsync(season.Id, cancellationToken))
            throw BusinessRule.Violation(nameof(request.PublicId), "This season still has rounds. Delete its rounds first.");

        await _season.DeleteAsync(season, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return request.PublicId;
    }
}
