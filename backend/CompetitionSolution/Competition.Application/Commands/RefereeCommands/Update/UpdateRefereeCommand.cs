using Competition.Application.DTOs.PersonDto;
using MediatR;

namespace Competition.Application.Commands.RefereeCommands.Update;

public class UpdateRefereeCommand : IRequest<RefereeDto>
{
    public UpdateRefereeCommand(string publicId, RefereeDto dto)
    {
        PublicId = publicId;
        FirstName = dto.FirstName;
        LastName = dto.LastName;
        DateOfBirth = dto.DateOfBirth;
        Picture = dto.Picture;
        CareerStart = dto.CareerStart;
    }

    public string PublicId { get; }
    public string FirstName { get; }
    public string LastName { get; }
    public string DateOfBirth { get; }
    public byte[]? Picture { get; }
    public string? CareerStart { get; }
}
