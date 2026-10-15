using Competition.Application.Common.Models;
using Competition.Application.DTOs.HistoryDto;
using Competition.Application.Helpers;
using Competition.Application.Interfaces;
using FluentValidation;
using MediatR;

namespace Competition.Application.Queries.TransferValueHistoryQueries;

public record GetTransferValueHistoriesQuery : IRequest<PagedResult<TransferValueHistoryDto>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public Guid? PlayerId { get; init; }
}

public class GetTransferValueHistoriesQueryValidator : AbstractValidator<GetTransferValueHistoriesQuery>
{
    public GetTransferValueHistoriesQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.PlayerId).NotEqual(Guid.Empty).When(x => x.PlayerId.HasValue);
    }
}

public class GetTransferValueHistoriesQueryHandler(IHistoryStore history)
    : IRequestHandler<GetTransferValueHistoriesQuery, PagedResult<TransferValueHistoryDto>>
{
    public async Task<PagedResult<TransferValueHistoryDto>> Handle(GetTransferValueHistoriesQuery request, CancellationToken cancellationToken)
    {
        var page = await history.GetTransfersAsync(request.PlayerId, request.Page, request.PageSize, cancellationToken);
        return new PagedResult<TransferValueHistoryDto>(page.Items.Select(HistoryMaps.ToDto).ToList(), page.TotalCount, page.Page, page.PageSize);
    }
}

public record GetTransferValueHistoryByIdQuery(Guid Id) : IRequest<TransferValueHistoryDto>;

public class GetTransferValueHistoryByIdQueryValidator : AbstractValidator<GetTransferValueHistoryByIdQuery>
{
    public GetTransferValueHistoryByIdQueryValidator() => RuleFor(x => x.Id).NotEmpty();
}

public class GetTransferValueHistoryByIdQueryHandler(IHistoryStore history)
    : IRequestHandler<GetTransferValueHistoryByIdQuery, TransferValueHistoryDto>
{
    public async Task<TransferValueHistoryDto> Handle(GetTransferValueHistoryByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await history.GetTransferAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"TransferValueHistory with id {request.Id} not found");
        return HistoryMaps.ToDto(entity);
    }
}
