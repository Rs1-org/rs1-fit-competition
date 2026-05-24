namespace Competition.Application.DTOs.PersonDto;

public class ManagerDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string DateOfBirth { get; set; } = "00:00:00";
    public byte[]? Picture { get; set; }
    public string? CareerStart { get; set; }
}
