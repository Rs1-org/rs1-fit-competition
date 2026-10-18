using AutoMapper;
using Competition.Application.Common.Models;
using Competition.Application.Common.Validation;
using Competition.Application.DTOs.StatisticDto;
using Competition.Application.Interfaces;
using FluentValidation;
using MediatR;
using NodaTime;

namespace Competition.Application.Queries.Statistics;

public class GetStatisticsQuery : IRequest<PagedResult<StatisticDto>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? MatchPublicId { get; init; }
    public string? SelectionPublicId { get; init; }
    public DateTimeOffset? DateFrom { get; init; }
    public DateTimeOffset? DateTo { get; init; }
    public int? MinTotalShots { get; init; }
}

public record GetStatisticQuery(string PublicId) : IRequest<StatisticDto>;
public record GetMatchStatisticQuery(string MatchPublicId) : IRequest<StatisticDto>;

public class GetStatisticsQueryValidator : AbstractValidator<GetStatisticsQuery>
{
    public GetStatisticsQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.MatchPublicId).MustBeValidPublicId().When(x => x.MatchPublicId is not null);
        RuleFor(x => x.SelectionPublicId).MustBeValidPublicId().When(x => x.SelectionPublicId is not null);
        RuleFor(x => x.MinTotalShots).GreaterThanOrEqualTo(0).When(x => x.MinTotalShots.HasValue);
        RuleFor(x => x.DateTo).GreaterThanOrEqualTo(x => x.DateFrom)
            .When(x => x.DateFrom.HasValue && x.DateTo.HasValue);
    }
}

public class GetStatisticQueryValidator : AbstractValidator<GetStatisticQuery>
{
    public GetStatisticQueryValidator() => RuleFor(x => x.PublicId).MustBeValidPublicId();
}

public class GetMatchStatisticQueryValidator : AbstractValidator<GetMatchStatisticQuery>
{
    public GetMatchStatisticQueryValidator() => RuleFor(x => x.MatchPublicId).MustBeValidPublicId();
}

public class GetStatisticsQueryHandler(IStatistic statistics, IMapper mapper)
    : IRequestHandler<GetStatisticsQuery, PagedResult<StatisticDto>>
{
    public async Task<PagedResult<StatisticDto>> Handle(GetStatisticsQuery request, CancellationToken cancellationToken)
    {
        var filter = new StatisticFilter(request.MatchPublicId, request.SelectionPublicId,
            request.DateFrom.HasValue ? Instant.FromDateTimeOffset(request.DateFrom.Value) : null,
            request.DateTo.HasValue ? Instant.FromDateTimeOffset(request.DateTo.Value) : null, request.MinTotalShots);
        var page = await statistics.GetFilteredAsync(filter, request.Page, request.PageSize, cancellationToken);
        return new PagedResult<StatisticDto>(mapper.Map<IReadOnlyList<StatisticDto>>(page.Items), page.TotalCount, page.Page, page.PageSize);
    }
}

public class GetStatisticQueryHandler(IStatistic statistics, IMapper mapper) : IRequestHandler<GetStatisticQuery, StatisticDto>
{
    public async Task<StatisticDto> Handle(GetStatisticQuery request, CancellationToken cancellationToken) =>
        mapper.Map<StatisticDto>(await statistics.GetDetailsAsync(request.PublicId, cancellationToken)
            ?? throw new KeyNotFoundException("Statistics not found."));
}

public class GetMatchStatisticQueryHandler(IStatistic statistics, IMapper mapper) : IRequestHandler<GetMatchStatisticQuery, StatisticDto>
{
    public async Task<StatisticDto> Handle(GetMatchStatisticQuery request, CancellationToken cancellationToken) =>
        mapper.Map<StatisticDto>(await statistics.GetForMatchAsync(request.MatchPublicId, cancellationToken)
            ?? throw new KeyNotFoundException("Statistics not found for this match."));
}
