using Competition.Application.Common.Models;
using Competition.Application.DTOs.HistoryDto;
using Competition.Application.Helpers;
using Competition.Application.Interfaces;
using FluentValidation;
using MediatR;

namespace Competition.Application.Queries.PlayerSelectionHistoryQueries;

public record GetPlayerSelectionHistoriesQuery : IRequest<PagedResult<PlayerSelectionHistoryDto>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public Guid? PlayerId { get; init; }
    public Guid? SelectionId { get; init; }
}

public class GetPlayerSelectionHistoriesQueryValidator : AbstractValidator<GetPlayerSelectionHistoriesQuery>
{
    public GetPlayerSelectionHistoriesQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.PlayerId).NotEqual(Guid.Empty).When(x => x.PlayerId.HasValue);
        RuleFor(x => x.SelectionId).NotEqual(Guid.Empty).When(x => x.SelectionId.HasValue);
    }
}

public class GetPlayerSelectionHistoriesQueryHandler(IHistoryStore history)
    : IRequestHandler<GetPlayerSelectionHistoriesQuery, PagedResult<PlayerSelectionHistoryDto>>
{
    public async Task<PagedResult<PlayerSelectionHistoryDto>> Handle(GetPlayerSelectionHistoriesQuery request, CancellationToken cancellationToken)
    {
        var page = await history.GetPlayerSelectionsAsync(request.PlayerId, request.SelectionId, request.Page, request.PageSize, cancellationToken);
        return new PagedResult<PlayerSelectionHistoryDto>(page.Items.Select(HistoryMaps.ToDto).ToList(), page.TotalCount, page.Page, page.PageSize);
    }
}

public record GetPlayerSelectionHistoryByIdQuery(Guid Id) : IRequest<PlayerSelectionHistoryDto>;

public class GetPlayerSelectionHistoryByIdQueryValidator : AbstractValidator<GetPlayerSelectionHistoryByIdQuery>
{
    public GetPlayerSelectionHistoryByIdQueryValidator() => RuleFor(x => x.Id).NotEmpty();
}

public class GetPlayerSelectionHistoryByIdQueryHandler(IHistoryStore history)
    : IRequestHandler<GetPlayerSelectionHistoryByIdQuery, PlayerSelectionHistoryDto>
{
    public async Task<PlayerSelectionHistoryDto> Handle(GetPlayerSelectionHistoryByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await history.GetPlayerSelectionAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"PlayerSelectionHistory with id {request.Id} not found");
        return HistoryMaps.ToDto(entity);
    }
}
