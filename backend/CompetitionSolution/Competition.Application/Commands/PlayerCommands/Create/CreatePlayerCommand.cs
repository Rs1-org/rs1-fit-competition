using Competition.Application.DTOs.PersonDto;
using MediatR;

namespace Competition.Application.Commands.PlayerCommands.Create;

public class CreatePlayerCommand : IRequest<PlayerDto>
{
    public CreatePlayerCommand(PlayerDto dto)
    {
        FirstName = dto.FirstName;
        LastName = dto.LastName;
        DateOfBirth = dto.DateOfBirth;
        Picture = dto.Picture;
        Height = dto.Height;
        ShirtNumber = dto.ShirtNumber;
        TransferValue = dto.TransferValue;
        PositionId = dto.PositionId;
        SubPositionId = dto.SubPositionId;
    }

    public string FirstName { get; }
    public string LastName { get; }
    public string DateOfBirth { get; }
    public byte[]? Picture { get; }
    public decimal Height { get; }
    public int ShirtNumber { get; }
    public float TransferValue { get; }
    public Guid? PositionId { get; }
    public Guid? SubPositionId { get; }
}
