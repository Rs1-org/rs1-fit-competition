using AutoMapper;
using Competition.Application.Common.Models;
using Competition.Application.DTOs.PersonDto;
using Competition.Application.Interfaces;
using Competition.Domain.Entities.FormationEntities;
using MediatR;

namespace Competition.Application.Queries.PlayerQueries.GetAll;

public class GetAllPlayersQueryHandler(IPlayer playerRepository, IMapper mapper, IPublicIdLookup lookup)
    : IRequestHandler<GetAllPlayersQuery, PagedResult<PlayerDto>>
{
    public async Task<PagedResult<PlayerDto>> Handle(GetAllPlayersQuery request, CancellationToken cancellationToken)
    {
        var positionId = await lookup.GetOptionalIdAsync<Position>(request.PositionPublicId, cancellationToken);
        var subPositionId = await lookup.GetOptionalIdAsync<SubPosition>(request.SubPositionPublicId, cancellationToken);
        var paged = await playerRepository.GetPagedAsync(
            new PlayerListFilter(request.Name, positionId, subPositionId, request.AgeMin, request.AgeMax),
            request.Page,
            request.PageSize,
            cancellationToken);

        var items = paged.Items.Select(mapper.Map<PlayerDto>).ToList();
        return new PagedResult<PlayerDto>(items, paged.TotalCount, paged.Page, paged.PageSize);
    }
}
