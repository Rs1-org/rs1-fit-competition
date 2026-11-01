using Competition.Application.Common.Models;
using Competition.Application.Common.Validation;
using Competition.Application.DTOs.HistoryDto;
using Competition.Application.Helpers;
using Competition.Application.Interfaces;
using Competition.Domain.Entities;
using Competition.Domain.Entities.Person;
using FluentValidation;
using MediatR;

namespace Competition.Application.Queries.PlayerSelectionHistoryQueries;

public record GetPlayerSelectionHistoriesQuery : IRequest<PagedResult<PlayerSelectionHistoryDto>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? PlayerPublicId { get; init; }
    public string? SelectionPublicId { get; init; }
}

public class GetPlayerSelectionHistoriesQueryValidator : AbstractValidator<GetPlayerSelectionHistoriesQuery>
{
    public GetPlayerSelectionHistoriesQueryValidator(IPublicIdLookup lookup)
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.PlayerPublicId!).MustReferenceExisting<GetPlayerSelectionHistoriesQuery, Player>(lookup)
            .When(x => !string.IsNullOrWhiteSpace(x.PlayerPublicId));
        RuleFor(x => x.SelectionPublicId!).MustReferenceExisting<GetPlayerSelectionHistoriesQuery, Selection>(lookup)
            .When(x => !string.IsNullOrWhiteSpace(x.SelectionPublicId));
    }
}

public class GetPlayerSelectionHistoriesQueryHandler(IHistoryStore history, IPublicIdLookup lookup)
    : IRequestHandler<GetPlayerSelectionHistoriesQuery, PagedResult<PlayerSelectionHistoryDto>>
{
    public async Task<PagedResult<PlayerSelectionHistoryDto>> Handle(GetPlayerSelectionHistoriesQuery request, CancellationToken cancellationToken)
    {
        var playerId = await lookup.GetOptionalIdAsync<Player>(request.PlayerPublicId, cancellationToken);
        var selectionId = await lookup.GetOptionalIdAsync<Selection>(request.SelectionPublicId, cancellationToken);
        var page = await history.GetPlayerSelectionsAsync(playerId, selectionId, request.Page, request.PageSize, cancellationToken);
        return new PagedResult<PlayerSelectionHistoryDto>(page.Items.Select(HistoryMaps.ToDto).ToList(), page.TotalCount, page.Page, page.PageSize);
    }
}

public record GetPlayerSelectionHistoryByIdQuery(string PublicId) : IRequest<PlayerSelectionHistoryDto>;

public class GetPlayerSelectionHistoryByIdQueryValidator : AbstractValidator<GetPlayerSelectionHistoryByIdQuery>
{
    public GetPlayerSelectionHistoryByIdQueryValidator() => RuleFor(x => x.PublicId).MustBeValidPublicId();
}

public class GetPlayerSelectionHistoryByIdQueryHandler(IHistoryStore history, IPublicIdLookup lookup)
    : IRequestHandler<GetPlayerSelectionHistoryByIdQuery, PlayerSelectionHistoryDto>
{
    public async Task<PlayerSelectionHistoryDto> Handle(GetPlayerSelectionHistoryByIdQuery request, CancellationToken cancellationToken)
    {
        var id = await lookup.GetRequiredIdAsync<PlayerSelectionHistory>(request.PublicId, cancellationToken);
        var entity = await history.GetPlayerSelectionAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"PlayerSelectionHistory with public id {request.PublicId} not found");
        return HistoryMaps.ToDto(entity);
    }
}
