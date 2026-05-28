using Competition.Application.DTOs.PersonDto;
using Competition.Application.Helpers;
using Competition.Application.Interfaces;
using MediatR;

namespace Competition.Application.Features.ManagerQueries.GetAll;

public class GetAllManagersQueryHandler(IManager managerRepository)
    : IRequestHandler<GetAllManagersQuery, IEnumerable<ManagerDto>>
{
    public async Task<IEnumerable<ManagerDto>> Handle(GetAllManagersQuery request, CancellationToken cancellationToken)
    {
        var managers = await managerRepository.GetAllActiveAsync(cancellationToken);
        return managers.Select(PersonMappingHelper.ToDto);
    }
}
