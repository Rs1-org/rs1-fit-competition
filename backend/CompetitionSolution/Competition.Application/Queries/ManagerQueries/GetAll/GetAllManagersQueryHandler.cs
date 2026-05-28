using AutoMapper;
using Competition.Application.DTOs.PersonDto;
using Competition.Application.Interfaces;
using MediatR;

namespace Competition.Application.Features.ManagerQueries.GetAll;

public class GetAllManagersQueryHandler(IManager managerRepository, IMapper mapper)
    : IRequestHandler<GetAllManagersQuery, IEnumerable<ManagerDto>>
{
    public async Task<IEnumerable<ManagerDto>> Handle(GetAllManagersQuery request, CancellationToken cancellationToken)
    {
        var managers = await managerRepository.GetAllActiveAsync(cancellationToken);
        return managers.Select(mapper.Map<ManagerDto>);
    }
}
