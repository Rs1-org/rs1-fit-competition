using Competition.Application.DTOs.PersonDto;
using Competition.Application.Helpers;
using Competition.Application.Interfaces;
using MediatR;

namespace Competition.Application.Queries.RefereeQueries.GetAll;

public class GetAllRefereesQueryHandler(IReferee refereeRepository)
    : IRequestHandler<GetAllRefereesQuery, IEnumerable<RefereeDto>>
{
    public async Task<IEnumerable<RefereeDto>> Handle(GetAllRefereesQuery request, CancellationToken cancellationToken)
    {
        var referees = await refereeRepository.GetAllActiveAsync(cancellationToken);
        return referees.Select(PersonMappingHelper.ToDto);
    }
}
