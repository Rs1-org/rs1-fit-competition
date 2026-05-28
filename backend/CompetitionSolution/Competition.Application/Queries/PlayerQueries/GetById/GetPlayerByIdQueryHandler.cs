using Competition.Application.DTOs.PersonDto;
using Competition.Application.Helpers;
using Competition.Application.Interfaces;
using MediatR;

namespace Competition.Application.Queries.PlayerQueries.GetById;

public class GetPlayerByIdQueryHandler(IPlayer playerRepository)
    : IRequestHandler<GetPlayerByIdQuery, PlayerDto?>
{
    public async Task<PlayerDto?> Handle(GetPlayerByIdQuery request, CancellationToken cancellationToken)
    {
        var player = await playerRepository.GetActiveByIdAsync(request.Id, cancellationToken);
        return player == null ? null : PersonMappingHelper.ToDto(player);
    }
}
