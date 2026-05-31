using Competition.Application.DTOs.PersonDto;
using MediatR;

namespace Competition.Application.Features.PlayerQueries.GetAll;

public class GetAllPlayersQuery : IRequest<IEnumerable<PlayerDto>>;
