using Competition.Application.DTOs.PersonDto;
using Competition.Application.Helpers;
using Competition.Application.Interfaces;
using MediatR;

namespace Competition.Application.Commands.PlayerCommands.Create;

public class CreatePlayerCommandHandler(IPlayer playerRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<CreatePlayerCommand, PlayerDto>
{
    public async Task<PlayerDto> Handle(CreatePlayerCommand request, CancellationToken cancellationToken)
    {
        var entity = PersonMappingHelper.ToEntity(request.Dto);
        await playerRepository.AddAsync(entity, cancellationToken);
        await unitOfWork.SaveChangesAsync();
        return PersonMappingHelper.ToDto(entity);
    }
}
