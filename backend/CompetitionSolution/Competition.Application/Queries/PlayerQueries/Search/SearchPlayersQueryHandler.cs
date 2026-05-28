using Competition.Application.DTOs.PersonDto;
using Competition.Application.Helpers;
using Competition.Application.Interfaces;
using MediatR;

namespace Competition.Application.Queries.PlayerQueries.Search;

public class SearchPlayersQueryHandler(IPlayer playerRepository)
    : IRequestHandler<SearchPlayersQuery, IEnumerable<PlayerDto>>
{
    public async Task<IEnumerable<PlayerDto>> Handle(SearchPlayersQuery request, CancellationToken cancellationToken)
    {
        var players = await playerRepository.GetAllActiveAsync(cancellationToken);

        return players
            .Where(p =>
                p.FirstName.Contains(request.Term, StringComparison.OrdinalIgnoreCase) ||
                p.LastName.Contains(request.Term, StringComparison.OrdinalIgnoreCase) ||
                p.ShirtNumber.ToString().Contains(request.Term, StringComparison.OrdinalIgnoreCase) ||
                p.Height.ToString().Contains(request.Term, StringComparison.OrdinalIgnoreCase))
            .Select(PersonMappingHelper.ToDto);
    }
}
