using AutoMapper;
using Competition.Application.DTOs.PersonDto;
using Competition.Application.Interfaces;
using Competition.Domain.Entities.Person;
using MediatR;

namespace Competition.Application.Queries.RefereeQueries.GetById;

public class GetRefereeByIdQueryHandler(IReferee refereeRepository, IMapper mapper, IPublicIdLookup lookup)
    : IRequestHandler<GetRefereeByIdQuery, RefereeDto?>
{
    public async Task<RefereeDto?> Handle(GetRefereeByIdQuery request, CancellationToken cancellationToken)
    {
        var id = await lookup.GetRequiredIdAsync<Referee>(request.PublicId, cancellationToken);
        var referee = await refereeRepository.GetActiveByIdAsync(id, cancellationToken);
        return referee == null ? null : mapper.Map<RefereeDto>(referee);
    }
}
