using AutoMapper;
using Competition.Application.DTOs.PersonDto;
using Competition.Application.Helpers;
using Competition.Application.Interfaces;
using Competition.Domain.Entities.FormationEntities;
using Competition.Domain.Entities.Person;
using MediatR;

namespace Competition.Application.Commands.PlayerCommands.Create;

public class CreatePlayerCommandHandler(IPlayer playerRepository, IPosition positions, ISubPosition subPositions, IUnitOfWork unitOfWork, IMapper mapper, IPublicIdLookup lookup)
    : IRequestHandler<CreatePlayerCommand, PlayerDto>
{
    public async Task<PlayerDto> Handle(CreatePlayerCommand request, CancellationToken cancellationToken)
    {
        var positionId = await lookup.GetOptionalIdAsync<Position>(request.PositionPublicId, cancellationToken);
        var subPositionId = await lookup.GetOptionalIdAsync<SubPosition>(request.SubPositionPublicId, cancellationToken);
        var position = positionId.HasValue ? await positions.GetByIdAsync(positionId.Value, cancellationToken) : null;
        var subPosition = subPositionId.HasValue ? await subPositions.GetWithPositionAsync(subPositionId.Value, cancellationToken) : null;
        PlayerPositionRules.EnsureConsistent(positionId, position, subPositionId, subPosition);

        var entity = mapper.Map<Player>(request);
        entity.PositionId = positionId;
        entity.SubPositionId = subPositionId;
        if (entity.Id == Guid.Empty)
            entity.Id = Guid.NewGuid();
        await playerRepository.AddAsync(entity, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var created = await playerRepository.GetActiveByIdAsync(entity.Id, cancellationToken);
        return mapper.Map<PlayerDto>(created);
    }
}
