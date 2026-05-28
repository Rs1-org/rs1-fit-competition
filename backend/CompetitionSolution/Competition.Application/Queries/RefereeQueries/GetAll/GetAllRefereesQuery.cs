using Competition.Application.DTOs.PersonDto;
using MediatR;

namespace Competition.Application.Queries.RefereeQueries.GetAll;

public class GetAllRefereesQuery : IRequest<IEnumerable<RefereeDto>>;
