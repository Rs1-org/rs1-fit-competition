using AutoMapper;
using Competition.Application.DTOs.PersonDto;
using Competition.Application.Interfaces;
using MediatR;

namespace Competition.Application.Features.PlayerQueries.GetAll;

public class GetAllPlayersQueryHandler(IPlayer playerRepository, IMapper mapper)
    : IRequestHandler<GetAllPlayersQuery, IEnumerable<PlayerDto>>
{
    public async Task<IEnumerable<PlayerDto>> Handle(GetAllPlayersQuery request, CancellationToken cancellationToken)
    {
        var players = await playerRepository.GetAllActiveAsync(cancellationToken);
        return players.Select(mapper.Map<PlayerDto>);
    }
}
