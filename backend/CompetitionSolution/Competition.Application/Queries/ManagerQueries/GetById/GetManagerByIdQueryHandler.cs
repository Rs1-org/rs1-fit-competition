using Competition.Application.DTOs.PersonDto;
using Competition.Application.Helpers;
using Competition.Application.Interfaces;
using MediatR;

namespace Competition.Application.Features.ManagerQueries.GetById;

public class GetManagerByIdQueryHandler(IManager managerRepository)
    : IRequestHandler<GetManagerByIdQuery, ManagerDto?>
{
    public async Task<ManagerDto?> Handle(GetManagerByIdQuery request, CancellationToken cancellationToken)
    {
        var manager = await managerRepository.GetActiveByIdAsync(request.Id, cancellationToken);
        return manager == null ? null : PersonMappingHelper.ToDto(manager);
    }
}
