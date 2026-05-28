using Competition.Application.DTOs.PersonDto;
using Competition.Application.Helpers;
using Competition.Application.Interfaces;
using MediatR;

namespace Competition.Application.Queries.RefereeQueries.GetById;

public class GetRefereeByIdQueryHandler(IReferee refereeRepository)
    : IRequestHandler<GetRefereeByIdQuery, RefereeDto?>
{
    public async Task<RefereeDto?> Handle(GetRefereeByIdQuery request, CancellationToken cancellationToken)
    {
        var referee = await refereeRepository.GetActiveByIdAsync(request.Id, cancellationToken);
        return referee == null ? null : PersonMappingHelper.ToDto(referee);
    }
}
