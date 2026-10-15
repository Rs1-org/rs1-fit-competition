using Competition.Application.Common.Models;
using Competition.Application.DTOs.HistoryDto;
using Competition.Application.Helpers;
using Competition.Application.Interfaces;
using FluentValidation;
using MediatR;

namespace Competition.Application.Queries.SelectionManagerHistoryQueries;

public record GetSelectionManagerHistoriesQuery : IRequest<PagedResult<SelectionManagerHistoryDto>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public Guid? ManagerId { get; init; }
    public Guid? SelectionId { get; init; }
}

public class GetSelectionManagerHistoriesQueryValidator : AbstractValidator<GetSelectionManagerHistoriesQuery>
{
    public GetSelectionManagerHistoriesQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.ManagerId).NotEqual(Guid.Empty).When(x => x.ManagerId.HasValue);
        RuleFor(x => x.SelectionId).NotEqual(Guid.Empty).When(x => x.SelectionId.HasValue);
    }
}

public class GetSelectionManagerHistoriesQueryHandler(IHistoryStore history)
    : IRequestHandler<GetSelectionManagerHistoriesQuery, PagedResult<SelectionManagerHistoryDto>>
{
    public async Task<PagedResult<SelectionManagerHistoryDto>> Handle(GetSelectionManagerHistoriesQuery request, CancellationToken cancellationToken)
    {
        var page = await history.GetManagerSelectionsAsync(request.ManagerId, request.SelectionId, request.Page, request.PageSize, cancellationToken);
        return new PagedResult<SelectionManagerHistoryDto>(page.Items.Select(HistoryMaps.ToDto).ToList(), page.TotalCount, page.Page, page.PageSize);
    }
}

public record GetSelectionManagerHistoryByIdQuery(Guid Id) : IRequest<SelectionManagerHistoryDto>;

public class GetSelectionManagerHistoryByIdQueryValidator : AbstractValidator<GetSelectionManagerHistoryByIdQuery>
{
    public GetSelectionManagerHistoryByIdQueryValidator() => RuleFor(x => x.Id).NotEmpty();
}

public class GetSelectionManagerHistoryByIdQueryHandler(IHistoryStore history)
    : IRequestHandler<GetSelectionManagerHistoryByIdQuery, SelectionManagerHistoryDto>
{
    public async Task<SelectionManagerHistoryDto> Handle(GetSelectionManagerHistoryByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await history.GetManagerSelectionAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"SelectionManagerHistory with id {request.Id} not found");
        return HistoryMaps.ToDto(entity);
    }
}
