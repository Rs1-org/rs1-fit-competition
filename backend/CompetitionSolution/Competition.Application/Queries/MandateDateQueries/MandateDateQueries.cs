using Competition.Application.Common.Models;
using Competition.Application.Common.Validation;
using Competition.Application.DTOs.HistoryDto;
using Competition.Application.Helpers;
using Competition.Application.Interfaces;
using Competition.Domain.Entities;
using Competition.Domain.Entities.Person;
using FluentValidation;
using MediatR;

namespace Competition.Application.Queries.MandateDateQueries;

public record GetMandateDatesQuery : IRequest<PagedResult<MandateDateDto>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? PlayerPublicId { get; init; }
    public string? ManagerPublicId { get; init; }
    public string? SelectionPublicId { get; init; }
    public string? PlayerSelectionHistoryPublicId { get; init; }
    public string? SelectionManagerHistoryPublicId { get; init; }
}

public class GetMandateDatesQueryValidator : AbstractValidator<GetMandateDatesQuery>
{
    public GetMandateDatesQueryValidator(IPublicIdLookup lookup)
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.PlayerPublicId!).MustReferenceExisting<GetMandateDatesQuery, Player>(lookup).When(x => !string.IsNullOrWhiteSpace(x.PlayerPublicId));
        RuleFor(x => x.ManagerPublicId!).MustReferenceExisting<GetMandateDatesQuery, Manager>(lookup).When(x => !string.IsNullOrWhiteSpace(x.ManagerPublicId));
        RuleFor(x => x.SelectionPublicId!).MustReferenceExisting<GetMandateDatesQuery, Selection>(lookup).When(x => !string.IsNullOrWhiteSpace(x.SelectionPublicId));
        RuleFor(x => x.PlayerSelectionHistoryPublicId!).MustReferenceExisting<GetMandateDatesQuery, PlayerSelectionHistory>(lookup).When(x => !string.IsNullOrWhiteSpace(x.PlayerSelectionHistoryPublicId));
        RuleFor(x => x.SelectionManagerHistoryPublicId!).MustReferenceExisting<GetMandateDatesQuery, SelectionManagerHistory>(lookup).When(x => !string.IsNullOrWhiteSpace(x.SelectionManagerHistoryPublicId));
    }
}

public class GetMandateDatesQueryHandler(IHistoryStore history, IPublicIdLookup lookup)
    : IRequestHandler<GetMandateDatesQuery, PagedResult<MandateDateDto>>
{
    public async Task<PagedResult<MandateDateDto>> Handle(GetMandateDatesQuery request, CancellationToken cancellationToken)
    {
        var page = await history.GetMandatesAsync(
            await lookup.GetOptionalIdAsync<Player>(request.PlayerPublicId, cancellationToken),
            await lookup.GetOptionalIdAsync<Manager>(request.ManagerPublicId, cancellationToken),
            await lookup.GetOptionalIdAsync<Selection>(request.SelectionPublicId, cancellationToken),
            await lookup.GetOptionalIdAsync<PlayerSelectionHistory>(request.PlayerSelectionHistoryPublicId, cancellationToken),
            await lookup.GetOptionalIdAsync<SelectionManagerHistory>(request.SelectionManagerHistoryPublicId, cancellationToken),
            request.Page,
            request.PageSize,
            cancellationToken);
        return new PagedResult<MandateDateDto>(page.Items.Select(HistoryMaps.ToDto).ToList(), page.TotalCount, page.Page, page.PageSize);
    }
}

public record GetMandateDateByIdQuery(string PublicId) : IRequest<MandateDateDto>;

public class GetMandateDateByIdQueryValidator : AbstractValidator<GetMandateDateByIdQuery>
{
    public GetMandateDateByIdQueryValidator() => RuleFor(x => x.PublicId).MustBeValidPublicId();
}

public class GetMandateDateByIdQueryHandler(IHistoryStore history, IPublicIdLookup lookup)
    : IRequestHandler<GetMandateDateByIdQuery, MandateDateDto>
{
    public async Task<MandateDateDto> Handle(GetMandateDateByIdQuery request, CancellationToken cancellationToken)
    {
        var id = await lookup.GetRequiredIdAsync<MandateDate>(request.PublicId, cancellationToken);
        var entity = await history.GetMandateAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"MandateDate with public id {request.PublicId} not found");
        return HistoryMaps.ToDto(entity);
    }
}
