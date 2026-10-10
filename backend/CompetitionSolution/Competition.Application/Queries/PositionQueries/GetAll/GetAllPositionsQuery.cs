using Competition.Application.Commands.PositionCommands.Create;
using Competition.Application.Common.Models;
using Competition.Application.DTOs.FormationDto;
using Competition.Application.Interfaces;
using MediatR;

namespace Competition.Application.Queries.PositionQueries.GetAll;

public class GetAllPositionsQuery : IRequest<PagedResult<PositionDto>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? Search { get; init; }
}

public class GetAllPositionsQueryHandler : IRequestHandler<GetAllPositionsQuery, PagedResult<PositionDto>>
{
    private readonly IPosition _positions;

    public GetAllPositionsQueryHandler(IPosition positions) => _positions = positions;

    public async Task<PagedResult<PositionDto>> Handle(GetAllPositionsQuery request, CancellationToken cancellationToken)
    {
        var paged = await _positions.GetPagedAsync(request.Page, request.PageSize, request.Search, cancellationToken);
        var items = paged.Items.Select(CreatePositionCommandHandler.Map).ToList();
        return new PagedResult<PositionDto>(items, paged.TotalCount, paged.Page, paged.PageSize);
    }
}
