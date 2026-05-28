using Competition.Application.DTOs.PersonDto;
using MediatR;

namespace Competition.Application.Features.RefereeQueries.GetById;

public class GetRefereeByIdQuery(Guid id) : IRequest<RefereeDto?>
{
    public Guid Id { get; } = id;
}
