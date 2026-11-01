using AutoMapper;
using Competition.Application.DTOs.PersonDto;
using Competition.Application.Helpers;
using Competition.Application.Interfaces;
using Competition.Domain.Entities.FormationEntities;
using Competition.Domain.Entities.Person;
using MediatR;

namespace Competition.Application.Commands.PlayerCommands.Update;

public class UpdatePlayerCommandHandler(IPlayer playerRepository, IPosition positions, ISubPosition subPositions, IUnitOfWork unitOfWork, IMapper mapper, IPublicIdLookup lookup)
    : IRequestHandler<UpdatePlayerCommand, PlayerDto>
{
    public async Task<PlayerDto> Handle(UpdatePlayerCommand request, CancellationToken cancellationToken)
    {
        var id = await lookup.GetRequiredIdAsync<Player>(request.PublicId, cancellationToken);
        var player = await playerRepository.GetActiveByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Player with public id {request.PublicId} not found");

        var positionId = await lookup.GetOptionalIdAsync<Position>(request.PositionPublicId, cancellationToken);
        var subPositionId = await lookup.GetOptionalIdAsync<SubPosition>(request.SubPositionPublicId, cancellationToken);
        var position = positionId.HasValue ? await positions.GetByIdAsync(positionId.Value, cancellationToken) : null;
        var subPosition = subPositionId.HasValue ? await subPositions.GetWithPositionAsync(subPositionId.Value, cancellationToken) : null;
        PlayerPositionRules.EnsureConsistent(positionId, position, subPositionId, subPosition);

        mapper.Map(request, player);
        player.PositionId = positionId;
        player.SubPositionId = subPositionId;
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var updated = await playerRepository.GetActiveByIdAsync(player.Id, cancellationToken);
        return mapper.Map<PlayerDto>(updated);
    }
}
