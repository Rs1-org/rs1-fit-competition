using Competition.Application.DTOs.PersonDto;
using MediatR;

namespace Competition.Application.Queries.PlayerQueries.GetAll;

public class GetAllPlayersQuery : IRequest<IEnumerable<PlayerDto>>;
