using Competition.Application.DTOs.PersonDto;
using MediatR;

namespace Competition.Application.Queries.PlayerQueries.Search;

public class SearchPlayersQuery(string term) : IRequest<IEnumerable<PlayerDto>>
{
    public string Term { get; } = term;
}
