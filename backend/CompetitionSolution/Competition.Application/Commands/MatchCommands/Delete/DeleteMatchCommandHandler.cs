using Competition.Application.Interfaces;
using Competition.Domain.Entities.MatchEntities;
using MediatR;
using Competition.Application.Live;

namespace Competition.Application.Commands.MatchCommands.Delete;

public class DeleteMatchCommandHandler : IRequestHandler<DeleteMatchCommand, string>
{
    private readonly IMatch _match;
    private readonly IPublicIdLookup _lookup;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPublisher _publisher;

    public DeleteMatchCommandHandler(IMatch match, IPublicIdLookup lookup, IUnitOfWork unitOfWork, IPublisher publisher)
    {
        _match = match;
        _lookup = lookup;
        _unitOfWork = unitOfWork;
        _publisher = publisher;
    }

    public async Task<string> Handle(DeleteMatchCommand request, CancellationToken cancellationToken)
    {
        var match = await _lookup.GetRequiredEntityAsync<Match>(request.PublicId, cancellationToken);

        await _match.DeleteAsync(match, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _publisher.Publish(new MatchChangedNotification(match.PublicId, "deleted"), cancellationToken);
        return request.PublicId;
    }
}
