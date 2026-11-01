using Competition.Application.Commands.SubPositionCommands.Create;
using Competition.Application.Common.Models;
using Competition.Application.Common.Validation;
using Competition.Application.DTOs.FormationDto;
using Competition.Application.Interfaces;
using Competition.Domain.Entities.FormationEntities;
using FluentValidation;
using MediatR;

namespace Competition.Application.Queries.SubPositionQueries.GetAll;

public class GetAllSubPositionsQuery : IRequest<PagedResult<SubPositionDto>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? Search { get; init; }
    public string? PositionPublicId { get; init; }
}

public class GetAllSubPositionsQueryValidator : AbstractValidator<GetAllSubPositionsQuery>
{
    public GetAllSubPositionsQueryValidator(IPublicIdLookup lookup)
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.PositionPublicId!).MustReferenceExisting<GetAllSubPositionsQuery, Position>(lookup)
            .When(x => !string.IsNullOrWhiteSpace(x.PositionPublicId));
    }
}

public class GetAllSubPositionsQueryHandler : IRequestHandler<GetAllSubPositionsQuery, PagedResult<SubPositionDto>>
{
    private readonly ISubPosition _subPositions;
    private readonly IPublicIdLookup _lookup;

    public GetAllSubPositionsQueryHandler(ISubPosition subPositions, IPublicIdLookup lookup)
    {
        _subPositions = subPositions;
        _lookup = lookup;
    }

    public async Task<PagedResult<SubPositionDto>> Handle(GetAllSubPositionsQuery request, CancellationToken cancellationToken)
    {
        var positionId = await _lookup.GetOptionalIdAsync<Position>(request.PositionPublicId, cancellationToken);
        var paged = await _subPositions.GetPagedAsync(request.Page, request.PageSize, request.Search, positionId, cancellationToken);
        var items = paged.Items.Select(CreateSubPositionCommandHandler.Map).ToList();
        return new PagedResult<SubPositionDto>(items, paged.TotalCount, paged.Page, paged.PageSize);
    }
}
