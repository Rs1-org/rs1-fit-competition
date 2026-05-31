using AutoMapper;
using Competition.Application.DTOs.PersonDto;
using Competition.Application.Interfaces;
using MediatR;

namespace Competition.Application.Features.RefereeQueries.GetAll;

public class GetAllRefereesQueryHandler(IReferee refereeRepository, IMapper mapper)
    : IRequestHandler<GetAllRefereesQuery, IEnumerable<RefereeDto>>
{
    public async Task<IEnumerable<RefereeDto>> Handle(GetAllRefereesQuery request, CancellationToken cancellationToken)
    {
        var referees = await refereeRepository.GetAllActiveAsync(cancellationToken);
        return referees.Select(mapper.Map<RefereeDto>);
    }
}
