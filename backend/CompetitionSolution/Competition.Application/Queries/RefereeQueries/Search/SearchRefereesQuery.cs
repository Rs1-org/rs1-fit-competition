using Competition.Application.DTOs.PersonDto;
using MediatR;

namespace Competition.Application.Features.RefereeQueries.Search;

public class SearchRefereesQuery(string term) : IRequest<IEnumerable<RefereeDto>>
{
    public string Term { get; } = term;
}
