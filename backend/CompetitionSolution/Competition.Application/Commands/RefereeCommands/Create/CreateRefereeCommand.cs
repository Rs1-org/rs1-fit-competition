using Competition.Application.DTOs.PersonDto;
using MediatR;

namespace Competition.Application.Features.RefereeCommands.Create;

public class CreateRefereeCommand : IRequest<RefereeDto>
{
    public CreateRefereeCommand(RefereeDto dto)
    {
        FirstName = dto.FirstName;
        LastName = dto.LastName;
        DateOfBirth = dto.DateOfBirth;
        Picture = dto.Picture;
        CareerStart = dto.CareerStart;
    }

    public string FirstName { get; }
    public string LastName { get; }
    public string DateOfBirth { get; }
    public byte[]? Picture { get; }
    public string? CareerStart { get; }
}
