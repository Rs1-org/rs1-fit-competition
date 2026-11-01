using Competition.Application.Common.Models;
using Competition.Application.Common.Validation;
using Competition.Application.DTOs.HistoryDto;
using Competition.Application.Helpers;
using Competition.Application.Interfaces;
using Competition.Domain.Entities;
using Competition.Domain.Entities.Person;
using FluentValidation;
using MediatR;

namespace Competition.Application.Queries.TransferValueHistoryQueries;

public record GetTransferValueHistoriesQuery : IRequest<PagedResult<TransferValueHistoryDto>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? PlayerPublicId { get; init; }
}

public class GetTransferValueHistoriesQueryValidator : AbstractValidator<GetTransferValueHistoriesQuery>
{
    public GetTransferValueHistoriesQueryValidator(IPublicIdLookup lookup)
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.PlayerPublicId!).MustReferenceExisting<GetTransferValueHistoriesQuery, Player>(lookup)
            .When(x => !string.IsNullOrWhiteSpace(x.PlayerPublicId));
    }
}

public class GetTransferValueHistoriesQueryHandler(IHistoryStore history, IPublicIdLookup lookup)
    : IRequestHandler<GetTransferValueHistoriesQuery, PagedResult<TransferValueHistoryDto>>
{
    public async Task<PagedResult<TransferValueHistoryDto>> Handle(GetTransferValueHistoriesQuery request, CancellationToken cancellationToken)
    {
        var playerId = await lookup.GetOptionalIdAsync<Player>(request.PlayerPublicId, cancellationToken);
        var page = await history.GetTransfersAsync(playerId, request.Page, request.PageSize, cancellationToken);
        return new PagedResult<TransferValueHistoryDto>(page.Items.Select(HistoryMaps.ToDto).ToList(), page.TotalCount, page.Page, page.PageSize);
    }
}

public record GetTransferValueHistoryByIdQuery(string PublicId) : IRequest<TransferValueHistoryDto>;

public class GetTransferValueHistoryByIdQueryValidator : AbstractValidator<GetTransferValueHistoryByIdQuery>
{
    public GetTransferValueHistoryByIdQueryValidator() => RuleFor(x => x.PublicId).MustBeValidPublicId();
}

public class GetTransferValueHistoryByIdQueryHandler(IHistoryStore history, IPublicIdLookup lookup)
    : IRequestHandler<GetTransferValueHistoryByIdQuery, TransferValueHistoryDto>
{
    public async Task<TransferValueHistoryDto> Handle(GetTransferValueHistoryByIdQuery request, CancellationToken cancellationToken)
    {
        var id = await lookup.GetRequiredIdAsync<TransferValueHistory>(request.PublicId, cancellationToken);
        var entity = await history.GetTransferAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"TransferValueHistory with public id {request.PublicId} not found");
        return HistoryMaps.ToDto(entity);
    }
}
