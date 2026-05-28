using Competition.Application.DTOs.PersonDto;
using MediatR;

namespace Competition.Application.Features.RefereeCommands.Update;

public class UpdateRefereeCommand : IRequest<RefereeDto>
{
    public UpdateRefereeCommand(Guid id, RefereeDto dto)
    {
        Id = id;
        FirstName = dto.FirstName;
        LastName = dto.LastName;
        DateOfBirth = dto.DateOfBirth;
        Picture = dto.Picture;
        CareerStart = dto.CareerStart;
    }

    public Guid Id { get; }
    public string FirstName { get; }
    public string LastName { get; }
    public string DateOfBirth { get; }
    public byte[]? Picture { get; }
    public string? CareerStart { get; }
}
