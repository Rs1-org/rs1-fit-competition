using Competition.Application.Common.Models;
using Competition.Application.DTOs.MatchDto;
using FluentValidation;
using MediatR;

namespace Competition.Application.Queries.Match.GetStatuses;

public class GetAllMatchStatusesQuery : IRequest<PagedResult<MatchStatusListDto>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

public class GetAllMatchStatusesQueryValidator : AbstractValidator<GetAllMatchStatusesQuery>
{
    public GetAllMatchStatusesQueryValidator()
    {
        RuleFor(s => s.Page).GreaterThanOrEqualTo(1);
        RuleFor(s => s.PageSize).InclusiveBetween(1, 100);
    }
}
