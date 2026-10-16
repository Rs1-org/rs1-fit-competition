using Competition.Application.Helpers;
using Competition.Application.Interfaces;
using Competition.Domain.Entities.CompetitionEntities;
using MediatR;

namespace Competition.Application.Commands.RoundCommands.Delete;

public class DeleteRoundCommandHandler : IRequestHandler<DeleteRoundCommand, string>
{
    private readonly IRound _round;
    private readonly IPublicIdLookup _lookup;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteRoundCommandHandler(IRound round, IPublicIdLookup lookup, IUnitOfWork unitOfWork)
    {
        _lookup = lookup;
        _round = round;
        _unitOfWork = unitOfWork;
    }

    public async Task<string> Handle(DeleteRoundCommand request, CancellationToken cancellationToken)
    {
        var round = await _lookup.GetRequiredEntityAsync<Round>(request.PublicId, cancellationToken);

        // Block instead of cascading so no match loses its round.
        if (await _round.HasMatchesAsync(round.Id, cancellationToken))
            throw BusinessRule.Violation(nameof(request.PublicId), "This round still has matches. Move or delete its matches first.");

        await _round.DeleteAsync(round, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return request.PublicId;
    }
}
