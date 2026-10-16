using Competition.Application.Interfaces;
using Competition.Domain.Entities.MatchEntities;
using MediatR;

namespace Competition.Application.Commands.MatchCommands.Delete;

public class DeleteMatchCommandHandler : IRequestHandler<DeleteMatchCommand, string>
{
    private readonly IMatch _match;
    private readonly IPublicIdLookup _lookup;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteMatchCommandHandler(IMatch match, IPublicIdLookup lookup, IUnitOfWork unitOfWork)
    {
        _match = match;
        _lookup = lookup;
        _unitOfWork = unitOfWork;
    }

    public async Task<string> Handle(DeleteMatchCommand request, CancellationToken cancellationToken)
    {
        var match = await _lookup.GetRequiredEntityAsync<Match>(request.PublicId, cancellationToken);

        await _match.DeleteAsync(match, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return request.PublicId;
    }
}
