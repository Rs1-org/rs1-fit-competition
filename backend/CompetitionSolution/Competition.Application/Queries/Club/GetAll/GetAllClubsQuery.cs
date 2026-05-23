using Competition.Application.DTOs.ClubDto;
using MediatR;

namespace Competition.Application.Queries.Club.GetAll;

public class GetAllClubsQuery : IRequest<IEnumerable<ClubResponseDto>>
{
}
