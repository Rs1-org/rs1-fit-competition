using AutoMapper;
using Competition.Application.DTOs.PersonDto;
using Competition.Application.Interfaces;
using Competition.Domain.Entities.Person;
using MediatR;

namespace Competition.Application.Queries.PlayerQueries.GetById;

public class GetPlayerByIdQueryHandler(IPlayer playerRepository, IMapper mapper, IPublicIdLookup lookup)
    : IRequestHandler<GetPlayerByIdQuery, PlayerDto?>
{
    public async Task<PlayerDto?> Handle(GetPlayerByIdQuery request, CancellationToken cancellationToken)
    {
        var id = await lookup.GetRequiredIdAsync<Player>(request.PublicId, cancellationToken);
        var player = await playerRepository.GetActiveByIdAsync(id, cancellationToken);
        return player == null ? null : mapper.Map<PlayerDto>(player);
    }
}
