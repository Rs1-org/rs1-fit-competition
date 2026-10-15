using Competition.Application.Common.Models;
using Competition.Application.DTOs.HistoryDto;
using Competition.Application.Helpers;
using Competition.Application.Interfaces;
using FluentValidation;
using MediatR;

namespace Competition.Application.Queries.MandateDateQueries;

public record GetMandateDatesQuery : IRequest<PagedResult<MandateDateDto>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public Guid? PlayerId { get; init; }
    public Guid? ManagerId { get; init; }
    public Guid? SelectionId { get; init; }
    public Guid? PlayerSelectionHistoryId { get; init; }
    public Guid? SelectionManagerHistoryId { get; init; }
}

public class GetMandateDatesQueryValidator : AbstractValidator<GetMandateDatesQuery>
{
    public GetMandateDatesQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.PlayerId).NotEqual(Guid.Empty).When(x => x.PlayerId.HasValue);
        RuleFor(x => x.ManagerId).NotEqual(Guid.Empty).When(x => x.ManagerId.HasValue);
        RuleFor(x => x.SelectionId).NotEqual(Guid.Empty).When(x => x.SelectionId.HasValue);
        RuleFor(x => x.PlayerSelectionHistoryId).NotEqual(Guid.Empty).When(x => x.PlayerSelectionHistoryId.HasValue);
        RuleFor(x => x.SelectionManagerHistoryId).NotEqual(Guid.Empty).When(x => x.SelectionManagerHistoryId.HasValue);
    }
}

public class GetMandateDatesQueryHandler(IHistoryStore history)
    : IRequestHandler<GetMandateDatesQuery, PagedResult<MandateDateDto>>
{
    public async Task<PagedResult<MandateDateDto>> Handle(GetMandateDatesQuery request, CancellationToken cancellationToken)
    {
        var page = await history.GetMandatesAsync(
            request.PlayerId,
            request.ManagerId,
            request.SelectionId,
            request.PlayerSelectionHistoryId,
            request.SelectionManagerHistoryId,
            request.Page,
            request.PageSize,
            cancellationToken);
        return new PagedResult<MandateDateDto>(page.Items.Select(HistoryMaps.ToDto).ToList(), page.TotalCount, page.Page, page.PageSize);
    }
}

public record GetMandateDateByIdQuery(Guid Id) : IRequest<MandateDateDto>;

public class GetMandateDateByIdQueryValidator : AbstractValidator<GetMandateDateByIdQuery>
{
    public GetMandateDateByIdQueryValidator() => RuleFor(x => x.Id).NotEmpty();
}

public class GetMandateDateByIdQueryHandler(IHistoryStore history)
    : IRequestHandler<GetMandateDateByIdQuery, MandateDateDto>
{
    public async Task<MandateDateDto> Handle(GetMandateDateByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await history.GetMandateAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"MandateDate with id {request.Id} not found");
        return HistoryMaps.ToDto(entity);
    }
}
