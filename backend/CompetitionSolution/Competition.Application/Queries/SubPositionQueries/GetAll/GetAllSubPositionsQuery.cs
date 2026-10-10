using Competition.Application.Commands.SubPositionCommands.Create;
using Competition.Application.Common.Models;
using Competition.Application.DTOs.FormationDto;
using Competition.Application.Interfaces;
using MediatR;

namespace Competition.Application.Queries.SubPositionQueries.GetAll;

public class GetAllSubPositionsQuery : IRequest<PagedResult<SubPositionDto>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? Search { get; init; }
    public Guid? PositionId { get; init; }
}

public class GetAllSubPositionsQueryHandler : IRequestHandler<GetAllSubPositionsQuery, PagedResult<SubPositionDto>>
{
    private readonly ISubPosition _subPositions;

    public GetAllSubPositionsQueryHandler(ISubPosition subPositions) => _subPositions = subPositions;

    public async Task<PagedResult<SubPositionDto>> Handle(GetAllSubPositionsQuery request, CancellationToken cancellationToken)
    {
        var paged = await _subPositions.GetPagedAsync(request.Page, request.PageSize, request.Search, request.PositionId, cancellationToken);
        var items = paged.Items.Select(CreateSubPositionCommandHandler.Map).ToList();
        return new PagedResult<SubPositionDto>(items, paged.TotalCount, paged.Page, paged.PageSize);
    }
}
