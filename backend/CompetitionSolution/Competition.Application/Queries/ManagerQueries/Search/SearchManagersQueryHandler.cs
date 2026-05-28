using AutoMapper;
using Competition.Application.DTOs.PersonDto;
using Competition.Application.Helpers;
using Competition.Application.Interfaces;
using MediatR;

namespace Competition.Application.Features.ManagerQueries.Search;

public class SearchManagersQueryHandler(IManager managerRepository, IMapper mapper)
    : IRequestHandler<SearchManagersQuery, IEnumerable<ManagerDto>>
{
    public async Task<IEnumerable<ManagerDto>> Handle(SearchManagersQuery request, CancellationToken cancellationToken)
    {
        var managers = await managerRepository.GetAllActiveAsync(cancellationToken);

        return managers
            .Where(m =>
                m.FirstName.Contains(request.Term, StringComparison.OrdinalIgnoreCase) ||
                m.LastName.Contains(request.Term, StringComparison.OrdinalIgnoreCase) ||
                (m.CareerStart != null &&
                 NodaTimeMappingHelper.FormatNullableLocalDate(m.CareerStart)!
                     .Contains(request.Term, StringComparison.OrdinalIgnoreCase)))
            .Select(mapper.Map<ManagerDto>);
    }
}
