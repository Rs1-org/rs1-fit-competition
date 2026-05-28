using Competition.Application.DTOs.PersonDto;
using Competition.Application.Helpers;
using Competition.Application.Interfaces;
using MediatR;

namespace Competition.Application.Commands.PlayerCommands.Update;

public class UpdatePlayerCommandHandler(IPlayer playerRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<UpdatePlayerCommand, PlayerDto>
{
    public async Task<PlayerDto> Handle(UpdatePlayerCommand request, CancellationToken cancellationToken)
    {
        var player = await playerRepository.GetActiveByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Player with id {request.Id} not found");

        PersonMappingHelper.Apply(request.Dto, player);
        await unitOfWork.SaveChangesAsync();
        return PersonMappingHelper.ToDto(player);
    }
}
