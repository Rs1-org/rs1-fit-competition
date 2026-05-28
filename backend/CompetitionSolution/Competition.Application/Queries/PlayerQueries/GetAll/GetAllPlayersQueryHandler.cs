using Competition.Application.DTOs.PersonDto;
using Competition.Application.Helpers;
using Competition.Application.Interfaces;
using MediatR;

namespace Competition.Application.Queries.PlayerQueries.GetAll;

public class GetAllPlayersQueryHandler(IPlayer playerRepository)
    : IRequestHandler<GetAllPlayersQuery, IEnumerable<PlayerDto>>
{
    public async Task<IEnumerable<PlayerDto>> Handle(GetAllPlayersQuery request, CancellationToken cancellationToken)
    {
        var players = await playerRepository.GetAllActiveAsync(cancellationToken);
        return players.Select(PersonMappingHelper.ToDto);
    }
}
