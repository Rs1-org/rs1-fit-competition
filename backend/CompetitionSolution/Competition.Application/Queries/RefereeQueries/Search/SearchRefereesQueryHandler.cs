using AutoMapper;
using Competition.Application.DTOs.PersonDto;
using Competition.Application.Helpers;
using Competition.Application.Interfaces;
using MediatR;

namespace Competition.Application.Features.RefereeQueries.Search;

public class SearchRefereesQueryHandler(IReferee refereeRepository, IMapper mapper)
    : IRequestHandler<SearchRefereesQuery, IEnumerable<RefereeDto>>
{
    public async Task<IEnumerable<RefereeDto>> Handle(SearchRefereesQuery request, CancellationToken cancellationToken)
    {
        var referees = await refereeRepository.GetAllActiveAsync(cancellationToken);

        return referees
            .Where(r =>
                r.FirstName.Contains(request.Term, StringComparison.OrdinalIgnoreCase) ||
                r.LastName.Contains(request.Term, StringComparison.OrdinalIgnoreCase) ||
                (r.CareerStart != null &&
                 NodaTimeMappingHelper.FormatNullableLocalDate(r.CareerStart)!
                     .Contains(request.Term, StringComparison.OrdinalIgnoreCase)))
            .Select(mapper.Map<RefereeDto>);
    }
}
