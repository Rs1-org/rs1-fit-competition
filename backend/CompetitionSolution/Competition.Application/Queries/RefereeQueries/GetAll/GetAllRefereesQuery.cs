using Competition.Application.DTOs.PersonDto;
using MediatR;

namespace Competition.Application.Features.RefereeQueries.GetAll;

public class GetAllRefereesQuery : IRequest<IEnumerable<RefereeDto>>;
