using Competition.Application.DTOs.PersonDto;
using MediatR;

namespace Competition.Application.Queries.PlayerQueries.GetById;

public class GetPlayerByIdQuery(Guid id) : IRequest<PlayerDto?>
{
    public Guid Id { get; } = id;
}
