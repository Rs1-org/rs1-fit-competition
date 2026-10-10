using Competition.Application.Commands.SelectionCommands.Create;
using Competition.Application.Common.Models;
using Competition.Application.DTOs.SelectionDto;
using Competition.Application.Interfaces;
using MediatR;

namespace Competition.Application.Queries.SelectionQueries.GetAll;

public class GetAllSelectionsQuery : IRequest<PagedResult<SelectionDto>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? Search { get; init; }
}

public class GetAllSelectionsQueryHandler : IRequestHandler<GetAllSelectionsQuery, PagedResult<SelectionDto>>
{
    private readonly ISelection _selections;

    public GetAllSelectionsQueryHandler(ISelection selections) => _selections = selections;

    public async Task<PagedResult<SelectionDto>> Handle(GetAllSelectionsQuery request, CancellationToken cancellationToken)
    {
        var paged = await _selections.GetPagedAsync(request.Page, request.PageSize, request.Search, cancellationToken);
        var items = paged.Items.Select(CreateSelectionCommandHandler.Map).ToList();
        return new PagedResult<SelectionDto>(items, paged.TotalCount, paged.Page, paged.PageSize);
    }
}
