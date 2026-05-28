using AutoMapper;
using Competition.Application.DTOs.PersonDto;
using Competition.Application.Interfaces;
using Competition.Domain.Entities.Person;
using MediatR;

namespace Competition.Application.Features.PlayerCommands.Create;

public class CreatePlayerCommandHandler(IPlayer playerRepository, IUnitOfWork unitOfWork, IMapper mapper)
    : IRequestHandler<CreatePlayerCommand, PlayerDto>
{
    public async Task<PlayerDto> Handle(CreatePlayerCommand request, CancellationToken cancellationToken)
    {
        var entity = mapper.Map<Player>(request);
        await playerRepository.AddAsync(entity, cancellationToken);
        await unitOfWork.SaveChangesAsync();
        return mapper.Map<PlayerDto>(entity);
    }
}
