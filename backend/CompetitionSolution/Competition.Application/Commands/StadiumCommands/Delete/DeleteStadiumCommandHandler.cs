using Competition.Application.Interfaces;
using Competition.Domain.Entities.MatchEntities;
using MediatR;

namespace Competition.Application.Commands.StadiumCommands.Delete;

public class DeleteStadiumCommandHandler : IRequestHandler<DeleteStadiumCommand, bool>
{
    private readonly IStadium _stadium;
    private readonly IGeoData _geoData;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPublicIdLookup _lookup;

    public DeleteStadiumCommandHandler(IStadium stadium, IGeoData geoData, IUnitOfWork unitOfWork, IPublicIdLookup lookup)
    {
        _lookup = lookup;
        _stadium = stadium;
        _geoData = geoData;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeleteStadiumCommand request, CancellationToken cancellationToken)
    {
        var stadium = await _lookup.GetRequiredEntityAsync<Stadium>(request.PublicId, cancellationToken);

        await _stadium.DeleteAsync(stadium, cancellationToken);

        // GeoData belongs to this stadium only, so it is soft-deleted together with it (after the stadium, its dependent).
        var geoData = await _geoData.GetByIdAsync(stadium.GeoDataId, cancellationToken);
        if (geoData != null)
            await _geoData.DeleteAsync(geoData, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
