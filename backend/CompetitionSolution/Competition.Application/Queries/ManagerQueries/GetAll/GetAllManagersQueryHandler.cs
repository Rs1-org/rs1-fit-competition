using AutoMapper;
using Competition.Application.Common.Models;
using Competition.Application.DTOs.PersonDto;
using Competition.Application.Interfaces;
using MediatR;
using NodaTime;

namespace Competition.Application.Queries.ManagerQueries.GetAll;

public class GetAllManagersQueryHandler(IManager managerRepository, IMapper mapper)
    : IRequestHandler<GetAllManagersQuery, PagedResult<ManagerDto>>
{
    public async Task<PagedResult<ManagerDto>> Handle(GetAllManagersQuery request, CancellationToken cancellationToken)
    {
        var paged = await managerRepository.GetPagedAsync(
            new StaffListFilter(request.Name, request.AgeMin, request.AgeMax, ToLocalDate(request.CareerStartFrom), ToLocalDate(request.CareerStartTo)),
            request.Page,
            request.PageSize,
            cancellationToken);

        var items = paged.Items.Select(mapper.Map<ManagerDto>).ToList();
        return new PagedResult<ManagerDto>(items, paged.TotalCount, paged.Page, paged.PageSize);
    }

    private static LocalDate? ToLocalDate(DateOnly? date) =>
        date is null ? null : LocalDate.FromDateOnly(date.Value);
}
