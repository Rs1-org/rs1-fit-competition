using AutoMapper;
using Competition.Application.DTOs.PersonDto;
using Competition.Application.Helpers;
using Competition.Application.Interfaces;
using Competition.Domain.Entities.Person;
using MediatR;

namespace Competition.Application.Features.PlayerCommands.Create;

public class CreatePlayerCommandHandler(IPlayer playerRepository, IPosition positions, ISubPosition subPositions, IUnitOfWork unitOfWork, IMapper mapper)
    : IRequestHandler<CreatePlayerCommand, PlayerDto>
{
    public async Task<PlayerDto> Handle(CreatePlayerCommand request, CancellationToken cancellationToken)
    {
        var position = request.PositionId.HasValue
            ? await positions.GetByIdAsync(request.PositionId.Value, cancellationToken)
            : null;
        var subPosition = request.SubPositionId.HasValue
            ? await subPositions.GetWithPositionAsync(request.SubPositionId.Value, cancellationToken)
            : null;
        PlayerPositionRules.EnsureConsistent(request.PositionId, position, request.SubPositionId, subPosition);

        var entity = mapper.Map<Player>(request);
        if (entity.Id == Guid.Empty)
            entity.Id = Guid.NewGuid();
        await playerRepository.AddAsync(entity, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var created = await playerRepository.GetActiveByIdAsync(entity.Id, cancellationToken);
        return mapper.Map<PlayerDto>(created);
    }
}
