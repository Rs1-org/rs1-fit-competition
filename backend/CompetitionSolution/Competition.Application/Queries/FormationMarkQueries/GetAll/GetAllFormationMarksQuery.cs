using Competition.Application.Commands.FormationMarkCommands.Create;
using Competition.Application.Common.Models;
using Competition.Application.DTOs.FormationDto;
using Competition.Application.Interfaces;
using MediatR;

namespace Competition.Application.Queries.FormationMarkQueries.GetAll;

public class GetAllFormationMarksQuery : IRequest<PagedResult<FormationMarkDto>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? Search { get; init; }
}

public class GetAllFormationMarksQueryHandler : IRequestHandler<GetAllFormationMarksQuery, PagedResult<FormationMarkDto>>
{
    private readonly IFormationMark _marks;

    public GetAllFormationMarksQueryHandler(IFormationMark marks) => _marks = marks;

    public async Task<PagedResult<FormationMarkDto>> Handle(GetAllFormationMarksQuery request, CancellationToken cancellationToken)
    {
        var paged = await _marks.GetPagedAsync(request.Page, request.PageSize, request.Search, cancellationToken);
        var items = paged.Items.Select(CreateFormationMarkCommandHandler.Map).ToList();
        return new PagedResult<FormationMarkDto>(items, paged.TotalCount, paged.Page, paged.PageSize);
    }
}
