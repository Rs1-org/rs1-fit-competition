using Competition.Application.DTOs.PersonDto;
using MediatR;

namespace Competition.Application.Commands.PlayerCommands.Update;

public class UpdatePlayerCommand : IRequest<PlayerDto>
{
    public UpdatePlayerCommand(string publicId, PlayerDto dto)
    {
        PublicId = publicId;
        FirstName = dto.FirstName;
        LastName = dto.LastName;
        DateOfBirth = dto.DateOfBirth;
        Picture = dto.Picture;
        Height = dto.Height;
        ShirtNumber = dto.ShirtNumber;
        TransferValue = dto.TransferValue;
        PositionPublicId = dto.PositionPublicId;
        SubPositionPublicId = dto.SubPositionPublicId;
    }

    public string PublicId { get; }
    public string FirstName { get; }
    public string LastName { get; }
    public string DateOfBirth { get; }
    public byte[]? Picture { get; }
    public decimal Height { get; }
    public int ShirtNumber { get; }
    public float TransferValue { get; }
    public string? PositionPublicId { get; }
    public string? SubPositionPublicId { get; }
}
