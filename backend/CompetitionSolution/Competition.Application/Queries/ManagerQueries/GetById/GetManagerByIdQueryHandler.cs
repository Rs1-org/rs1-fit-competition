using AutoMapper;
using Competition.Application.DTOs.PersonDto;
using Competition.Application.Interfaces;
using Competition.Domain.Entities.Person;
using MediatR;

namespace Competition.Application.Queries.ManagerQueries.GetById;

public class GetManagerByIdQueryHandler(IManager managerRepository, IMapper mapper, IPublicIdLookup lookup)
    : IRequestHandler<GetManagerByIdQuery, ManagerDto?>
{
    public async Task<ManagerDto?> Handle(GetManagerByIdQuery request, CancellationToken cancellationToken)
    {
        var id = await lookup.GetRequiredIdAsync<Manager>(request.PublicId, cancellationToken);
        var manager = await managerRepository.GetActiveByIdAsync(id, cancellationToken);
        return manager == null ? null : mapper.Map<ManagerDto>(manager);
    }
}
