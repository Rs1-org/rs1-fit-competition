using AutoMapper;
using Competition.Application.DTOs.PersonDto;
using Competition.Application.Helpers;
using Competition.Application.Interfaces;
using MediatR;

namespace Competition.Application.Features.PlayerCommands.Update;

public class UpdatePlayerCommandHandler(IPlayer playerRepository, IPosition positions, ISubPosition subPositions, IUnitOfWork unitOfWork, IMapper mapper)
    : IRequestHandler<UpdatePlayerCommand, PlayerDto>
{
    public async Task<PlayerDto> Handle(UpdatePlayerCommand request, CancellationToken cancellationToken)
    {
        var player = await playerRepository.GetActiveByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Player with id {request.Id} not found");

        var position = request.PositionId.HasValue
            ? await positions.GetByIdAsync(request.PositionId.Value, cancellationToken)
            : null;
        var subPosition = request.SubPositionId.HasValue
            ? await subPositions.GetWithPositionAsync(request.SubPositionId.Value, cancellationToken)
            : null;
        PlayerPositionRules.EnsureConsistent(request.PositionId, position, request.SubPositionId, subPosition);

        mapper.Map(request, player);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var updated = await playerRepository.GetActiveByIdAsync(player.Id, cancellationToken);
        return mapper.Map<PlayerDto>(updated);
    }
}
