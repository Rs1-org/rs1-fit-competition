using Competition.Application.DTOs.PersonDto;
using MediatR;

namespace Competition.Application.Features.ManagerQueries.GetById;

public class GetManagerByIdQuery(Guid id) : IRequest<ManagerDto?>
{
    public Guid Id { get; } = id;
}
