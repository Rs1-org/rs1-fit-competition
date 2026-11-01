using Competition.Application.Common.Models;
using Competition.Application.Common.Validation;
using Competition.Application.DTOs.HistoryDto;
using Competition.Application.Helpers;
using Competition.Application.Interfaces;
using Competition.Domain.Entities;
using Competition.Domain.Entities.Person;
using FluentValidation;
using MediatR;

namespace Competition.Application.Queries.SelectionManagerHistoryQueries;

public record GetSelectionManagerHistoriesQuery : IRequest<PagedResult<SelectionManagerHistoryDto>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? ManagerPublicId { get; init; }
    public string? SelectionPublicId { get; init; }
}

public class GetSelectionManagerHistoriesQueryValidator : AbstractValidator<GetSelectionManagerHistoriesQuery>
{
    public GetSelectionManagerHistoriesQueryValidator(IPublicIdLookup lookup)
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.ManagerPublicId!).MustReferenceExisting<GetSelectionManagerHistoriesQuery, Manager>(lookup).When(x => !string.IsNullOrWhiteSpace(x.ManagerPublicId));
        RuleFor(x => x.SelectionPublicId!).MustReferenceExisting<GetSelectionManagerHistoriesQuery, Selection>(lookup).When(x => !string.IsNullOrWhiteSpace(x.SelectionPublicId));
    }
}

public class GetSelectionManagerHistoriesQueryHandler(IHistoryStore history, IPublicIdLookup lookup)
    : IRequestHandler<GetSelectionManagerHistoriesQuery, PagedResult<SelectionManagerHistoryDto>>
{
    public async Task<PagedResult<SelectionManagerHistoryDto>> Handle(GetSelectionManagerHistoriesQuery request, CancellationToken cancellationToken)
    {
        var page = await history.GetManagerSelectionsAsync(
            await lookup.GetOptionalIdAsync<Manager>(request.ManagerPublicId, cancellationToken),
            await lookup.GetOptionalIdAsync<Selection>(request.SelectionPublicId, cancellationToken),
            request.Page,
            request.PageSize,
            cancellationToken);
        return new PagedResult<SelectionManagerHistoryDto>(page.Items.Select(HistoryMaps.ToDto).ToList(), page.TotalCount, page.Page, page.PageSize);
    }
}

public record GetSelectionManagerHistoryByIdQuery(string PublicId) : IRequest<SelectionManagerHistoryDto>;

public class GetSelectionManagerHistoryByIdQueryValidator : AbstractValidator<GetSelectionManagerHistoryByIdQuery>
{
    public GetSelectionManagerHistoryByIdQueryValidator() => RuleFor(x => x.PublicId).MustBeValidPublicId();
}

public class GetSelectionManagerHistoryByIdQueryHandler(IHistoryStore history, IPublicIdLookup lookup)
    : IRequestHandler<GetSelectionManagerHistoryByIdQuery, SelectionManagerHistoryDto>
{
    public async Task<SelectionManagerHistoryDto> Handle(GetSelectionManagerHistoryByIdQuery request, CancellationToken cancellationToken)
    {
        var id = await lookup.GetRequiredIdAsync<SelectionManagerHistory>(request.PublicId, cancellationToken);
        var entity = await history.GetManagerSelectionAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"SelectionManagerHistory with public id {request.PublicId} not found");
        return HistoryMaps.ToDto(entity);
    }
}
