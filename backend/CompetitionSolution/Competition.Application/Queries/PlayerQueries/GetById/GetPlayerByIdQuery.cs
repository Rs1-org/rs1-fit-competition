using Competition.Application.DTOs.PersonDto;
using MediatR;

namespace Competition.Application.Features.PlayerQueries.GetById;

public class GetPlayerByIdQuery(Guid id) : IRequest<PlayerDto?>
{
    public Guid Id { get; } = id;
}
