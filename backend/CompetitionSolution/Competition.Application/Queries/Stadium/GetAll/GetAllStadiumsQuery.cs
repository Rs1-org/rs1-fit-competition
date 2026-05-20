using Competition.Application.DTOs.StadiumDto;
using MediatR;

namespace Competition.Application.Queries.Stadium.GetAll;

public class GetAllStadiumsQuery : IRequest<IEnumerable<StadiumListDto>>
{
}
