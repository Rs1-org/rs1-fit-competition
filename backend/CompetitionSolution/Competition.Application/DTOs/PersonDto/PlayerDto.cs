namespace Competition.Application.DTOs.PersonDto;

public class PlayerDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string DateOfBirth { get; set; } = "00:00:00";
    public byte[]? Picture { get; set; }
    public decimal Height { get; set; }
    public int ShirtNumber { get; set; }
    public float TransferValue { get; set; }
    public Guid? PositionId { get; set; }
    public Guid? SubPositionId { get; set; }
    public string? PositionName { get; set; }
    public string? SubPositionName { get; set; }
}
