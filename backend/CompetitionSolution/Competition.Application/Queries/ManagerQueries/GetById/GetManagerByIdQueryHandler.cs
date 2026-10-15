using AutoMapper;
using Competition.Application.DTOs.PersonDto;
using Competition.Application.Interfaces;
using MediatR;

namespace Competition.Application.Queries.ManagerQueries.GetById;

public class GetManagerByIdQueryHandler(IManager managerRepository, IMapper mapper)
    : IRequestHandler<GetManagerByIdQuery, ManagerDto?>
{
    public async Task<ManagerDto?> Handle(GetManagerByIdQuery request, CancellationToken cancellationToken)
    {
        var manager = await managerRepository.GetActiveByIdAsync(request.Id, cancellationToken);
        return manager == null ? null : mapper.Map<ManagerDto>(manager);
    }
}
