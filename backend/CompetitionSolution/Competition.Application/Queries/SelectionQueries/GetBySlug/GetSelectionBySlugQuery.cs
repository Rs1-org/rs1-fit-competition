using Competition.Application.Commands.SelectionCommands.Create;
using Competition.Application.DTOs.SelectionDto;
using Competition.Application.Interfaces;
using MediatR;

namespace Competition.Application.Queries.SelectionQueries.GetBySlug;

public class GetSelectionBySlugQuery : IRequest<SelectionDto>
{
    public string Slug { get; set; } = string.Empty;
}

public class GetSelectionBySlugQueryHandler : IRequestHandler<GetSelectionBySlugQuery, SelectionDto>
{
    private readonly ISelection _selections;

    public GetSelectionBySlugQueryHandler(ISelection selections) => _selections = selections;

    public async Task<SelectionDto> Handle(GetSelectionBySlugQuery request, CancellationToken cancellationToken)
    {
        var entity = await _selections.GetBySlugAsync(request.Slug, cancellationToken)
            ?? throw new KeyNotFoundException($"Selection '{request.Slug}' was not found.");
        return CreateSelectionCommandHandler.Map(entity);
    }
}
