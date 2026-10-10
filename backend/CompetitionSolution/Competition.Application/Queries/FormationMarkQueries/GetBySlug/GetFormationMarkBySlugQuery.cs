using Competition.Application.Commands.FormationMarkCommands.Create;
using Competition.Application.DTOs.FormationDto;
using Competition.Application.Interfaces;
using MediatR;

namespace Competition.Application.Queries.FormationMarkQueries.GetBySlug;

public class GetFormationMarkBySlugQuery : IRequest<FormationMarkDto>
{
    public string Slug { get; set; } = string.Empty;
}

public class GetFormationMarkBySlugQueryHandler : IRequestHandler<GetFormationMarkBySlugQuery, FormationMarkDto>
{
    private readonly IFormationMark _marks;

    public GetFormationMarkBySlugQueryHandler(IFormationMark marks) => _marks = marks;

    public async Task<FormationMarkDto> Handle(GetFormationMarkBySlugQuery request, CancellationToken cancellationToken)
    {
        var entity = await _marks.GetBySlugAsync(request.Slug, cancellationToken)
            ?? throw new KeyNotFoundException($"Formation mark '{request.Slug}' was not found.");
        return CreateFormationMarkCommandHandler.Map(entity);
    }
}
