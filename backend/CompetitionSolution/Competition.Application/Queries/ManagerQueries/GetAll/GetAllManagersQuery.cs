using Competition.Application.DTOs.PersonDto;
using MediatR;

namespace Competition.Application.Features.ManagerQueries.GetAll;

public class GetAllManagersQuery : IRequest<IEnumerable<ManagerDto>>;
