using AutoMapper;
using Competition.Application.DTOs.PersonDto;
using Competition.Application.Interfaces;
using MediatR;

namespace Competition.Application.Features.RefereeQueries.GetById;

public class GetRefereeByIdQueryHandler(IReferee refereeRepository, IMapper mapper)
    : IRequestHandler<GetRefereeByIdQuery, RefereeDto?>
{
    public async Task<RefereeDto?> Handle(GetRefereeByIdQuery request, CancellationToken cancellationToken)
    {
        var referee = await refereeRepository.GetActiveByIdAsync(request.Id, cancellationToken);
        return referee == null ? null : mapper.Map<RefereeDto>(referee);
    }
}
