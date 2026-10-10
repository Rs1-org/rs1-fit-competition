using Competition.Application.DTOs.SelectionDto;
using Competition.Application.Interfaces;
using MediatR;

namespace Competition.Application.Queries.SelectionQueries.GetPlayerAppearances;

public class GetPlayerAppearancesQuery : IRequest<IReadOnlyList<PlayerAppearanceDto>>
{
    public Guid PlayerId { get; set; }
}

public class GetPlayerAppearancesQueryHandler : IRequestHandler<GetPlayerAppearancesQuery, IReadOnlyList<PlayerAppearanceDto>>
{
    private readonly IPlayerAppearances _appearances;

    public GetPlayerAppearancesQueryHandler(IPlayerAppearances appearances) => _appearances = appearances;

    public Task<IReadOnlyList<PlayerAppearanceDto>> Handle(GetPlayerAppearancesQuery request, CancellationToken cancellationToken) =>
        _appearances.ForPlayerAsync(request.PlayerId, cancellationToken);
}
