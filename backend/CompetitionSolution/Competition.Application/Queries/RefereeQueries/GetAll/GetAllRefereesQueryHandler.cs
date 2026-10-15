using AutoMapper;
using Competition.Application.Common.Models;
using Competition.Application.DTOs.PersonDto;
using Competition.Application.Interfaces;
using MediatR;
using NodaTime;

namespace Competition.Application.Features.RefereeQueries.GetAll;

public class GetAllRefereesQueryHandler(IReferee refereeRepository, IMapper mapper)
    : IRequestHandler<GetAllRefereesQuery, PagedResult<RefereeDto>>
{
    public async Task<PagedResult<RefereeDto>> Handle(GetAllRefereesQuery request, CancellationToken cancellationToken)
    {
        var paged = await refereeRepository.GetPagedAsync(
            new StaffListFilter(request.Name, request.AgeMin, request.AgeMax, ToLocalDate(request.CareerStartFrom), ToLocalDate(request.CareerStartTo)),
            request.Page,
            request.PageSize,
            cancellationToken);

        var items = paged.Items.Select(mapper.Map<RefereeDto>).ToList();
        return new PagedResult<RefereeDto>(items, paged.TotalCount, paged.Page, paged.PageSize);
    }

    private static LocalDate? ToLocalDate(DateOnly? date) =>
        date is null ? null : LocalDate.FromDateOnly(date.Value);
}
