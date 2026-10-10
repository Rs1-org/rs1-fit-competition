using Competition.Application.Commands.FormationCommands.Create;
using Competition.Application.DTOs.FormationDto;
using Competition.Application.Interfaces;
using MediatR;

namespace Competition.Application.Queries.FormationQueries.GetBySlug;

public class GetFormationBySlugQuery : IRequest<FormationDto>
{
    public string Slug { get; set; } = string.Empty;
}

public class GetFormationBySlugQueryHandler : IRequestHandler<GetFormationBySlugQuery, FormationDto>
{
    private readonly IFormation _formations;

    public GetFormationBySlugQueryHandler(IFormation formations) => _formations = formations;

    public async Task<FormationDto> Handle(GetFormationBySlugQuery request, CancellationToken cancellationToken)
    {
        var entity = await _formations.GetBySlugAsync(request.Slug, cancellationToken)
            ?? throw new KeyNotFoundException($"Formation '{request.Slug}' was not found.");
        return CreateFormationCommandHandler.Map(entity);
    }
}
