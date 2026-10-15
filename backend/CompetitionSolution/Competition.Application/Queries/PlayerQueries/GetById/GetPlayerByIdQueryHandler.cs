using AutoMapper;
using Competition.Application.DTOs.PersonDto;
using Competition.Application.Interfaces;
using MediatR;

namespace Competition.Application.Queries.PlayerQueries.GetById;

public class GetPlayerByIdQueryHandler(IPlayer playerRepository, IMapper mapper)
    : IRequestHandler<GetPlayerByIdQuery, PlayerDto?>
{
    public async Task<PlayerDto?> Handle(GetPlayerByIdQuery request, CancellationToken cancellationToken)
    {
        var player = await playerRepository.GetActiveByIdAsync(request.Id, cancellationToken);
        return player == null ? null : mapper.Map<PlayerDto>(player);
    }
}
