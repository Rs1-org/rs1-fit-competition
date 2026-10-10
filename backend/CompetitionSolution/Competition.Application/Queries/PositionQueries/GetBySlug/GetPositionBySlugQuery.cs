using Competition.Application.Commands.PositionCommands.Create;
using Competition.Application.DTOs.FormationDto;
using Competition.Application.Interfaces;
using MediatR;

namespace Competition.Application.Queries.PositionQueries.GetBySlug;

public class GetPositionBySlugQuery : IRequest<PositionDto>
{
    public string Slug { get; set; } = string.Empty;
}

public class GetPositionBySlugQueryHandler : IRequestHandler<GetPositionBySlugQuery, PositionDto>
{
    private readonly IPosition _positions;

    public GetPositionBySlugQueryHandler(IPosition positions) => _positions = positions;

    public async Task<PositionDto> Handle(GetPositionBySlugQuery request, CancellationToken cancellationToken)
    {
        var entity = await _positions.GetBySlugAsync(request.Slug, cancellationToken)
            ?? throw new KeyNotFoundException($"Position '{request.Slug}' was not found.");
        return CreatePositionCommandHandler.Map(entity);
    }
}
