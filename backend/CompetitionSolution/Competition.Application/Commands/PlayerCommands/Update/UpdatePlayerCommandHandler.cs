using AutoMapper;
using Competition.Application.DTOs.PersonDto;
using Competition.Application.Interfaces;
using MediatR;

namespace Competition.Application.Features.PlayerCommands.Update;

public class UpdatePlayerCommandHandler(IPlayer playerRepository, IUnitOfWork unitOfWork, IMapper mapper)
    : IRequestHandler<UpdatePlayerCommand, PlayerDto>
{
    public async Task<PlayerDto> Handle(UpdatePlayerCommand request, CancellationToken cancellationToken)
    {
        var player = await playerRepository.GetActiveByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Player with id {request.Id} not found");

        mapper.Map(request, player);
        await unitOfWork.SaveChangesAsync();
        return mapper.Map<PlayerDto>(player);
    }
}
