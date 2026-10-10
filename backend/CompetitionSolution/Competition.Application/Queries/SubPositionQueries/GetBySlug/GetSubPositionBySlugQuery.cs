using Competition.Application.Commands.SubPositionCommands.Create;
using Competition.Application.DTOs.FormationDto;
using Competition.Application.Interfaces;
using MediatR;

namespace Competition.Application.Queries.SubPositionQueries.GetBySlug;

public class GetSubPositionBySlugQuery : IRequest<SubPositionDto>
{
    public string Slug { get; set; } = string.Empty;
}

public class GetSubPositionBySlugQueryHandler : IRequestHandler<GetSubPositionBySlugQuery, SubPositionDto>
{
    private readonly ISubPosition _subPositions;

    public GetSubPositionBySlugQueryHandler(ISubPosition subPositions) => _subPositions = subPositions;

    public async Task<SubPositionDto> Handle(GetSubPositionBySlugQuery request, CancellationToken cancellationToken)
    {
        var entity = await _subPositions.GetBySlugAsync(request.Slug, cancellationToken)
            ?? throw new KeyNotFoundException($"Subposition '{request.Slug}' was not found.");
        return CreateSubPositionCommandHandler.Map(entity);
    }
}
