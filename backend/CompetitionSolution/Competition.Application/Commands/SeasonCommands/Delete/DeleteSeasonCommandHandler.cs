using Competition.Application.Helpers;
using Competition.Application.Interfaces;
using MediatR;

namespace Competition.Application.Commands.SeasonCommands.Delete;

public class DeleteSeasonCommandHandler : IRequestHandler<DeleteSeasonCommand, Guid>
{
    private readonly ISeason _season;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteSeasonCommandHandler(ISeason season, IUnitOfWork unitOfWork)
    {
        _season = season;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(DeleteSeasonCommand request, CancellationToken cancellationToken)
    {
        var season = await _season.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Season with ID '{request.Id}' not found.");

        // Block instead of cascading so no round is left without a parent.
        if (await _season.HasRoundsAsync(season.Id, cancellationToken))
            throw BusinessRule.Violation(nameof(request.Id), "This season still has rounds. Delete its rounds first.");

        await _season.DeleteAsync(season, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return request.Id;
    }
}
