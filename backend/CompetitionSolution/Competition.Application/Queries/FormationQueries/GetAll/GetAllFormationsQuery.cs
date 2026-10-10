using Competition.Application.Commands.FormationCommands.Create;
using Competition.Application.Common.Models;
using Competition.Application.DTOs.FormationDto;
using Competition.Application.Interfaces;
using MediatR;

namespace Competition.Application.Queries.FormationQueries.GetAll;

public class GetAllFormationsQuery : IRequest<PagedResult<FormationDto>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? Search { get; init; }
}

public class GetAllFormationsQueryHandler : IRequestHandler<GetAllFormationsQuery, PagedResult<FormationDto>>
{
    private readonly IFormation _formations;

    public GetAllFormationsQueryHandler(IFormation formations) => _formations = formations;

    public async Task<PagedResult<FormationDto>> Handle(GetAllFormationsQuery request, CancellationToken cancellationToken)
    {
        var paged = await _formations.GetPagedAsync(request.Page, request.PageSize, request.Search, cancellationToken);
        var items = paged.Items.Select(CreateFormationCommandHandler.Map).ToList();
        return new PagedResult<FormationDto>(items, paged.TotalCount, paged.Page, paged.PageSize);
    }
}
