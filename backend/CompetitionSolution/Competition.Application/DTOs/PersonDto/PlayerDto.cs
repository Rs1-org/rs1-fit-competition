namespace Competition.Application.DTOs.PersonDto;

public class PlayerDto
{
    public string PublicId { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string DateOfBirth { get; set; } = "00:00:00";
    public byte[]? Picture { get; set; }
    public decimal Height { get; set; }
    public int ShirtNumber { get; set; }
    public float TransferValue { get; set; }
    public string? PositionPublicId { get; set; }
    public string? SubPositionPublicId { get; set; }
    public string? PositionName { get; set; }
    public string? SubPositionName { get; set; }
}
