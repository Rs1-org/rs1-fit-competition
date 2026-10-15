using AutoMapper;
using Competition.Application.Common.Models;
using Competition.Application.DTOs.PersonDto;
using Competition.Application.Interfaces;
using MediatR;

namespace Competition.Application.Features.PlayerQueries.GetAll;

public class GetAllPlayersQueryHandler(IPlayer playerRepository, IMapper mapper)
    : IRequestHandler<GetAllPlayersQuery, PagedResult<PlayerDto>>
{
    public async Task<PagedResult<PlayerDto>> Handle(GetAllPlayersQuery request, CancellationToken cancellationToken)
    {
        var paged = await playerRepository.GetPagedAsync(
            new PlayerListFilter(request.Name, request.PositionId, request.SubPositionId, request.AgeMin, request.AgeMax),
            request.Page,
            request.PageSize,
            cancellationToken);

        var items = paged.Items.Select(mapper.Map<PlayerDto>).ToList();
        return new PagedResult<PlayerDto>(items, paged.TotalCount, paged.Page, paged.PageSize);
    }
}
