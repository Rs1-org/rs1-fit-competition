using Competition.Application.DTOs.PersonDto;
using MediatR;

namespace Competition.Application.Features.ManagerQueries.Search;

public class SearchManagersQuery(string term) : IRequest<IEnumerable<ManagerDto>>
{
    public string Term { get; } = term;
}
