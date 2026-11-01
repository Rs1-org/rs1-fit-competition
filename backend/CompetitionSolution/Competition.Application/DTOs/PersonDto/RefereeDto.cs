namespace Competition.Application.DTOs.PersonDto;

public class RefereeDto
{
    public string PublicId { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string DateOfBirth { get; set; } = "00:00:00";
    public byte[]? Picture { get; set; }
    public string? CareerStart { get; set; }
}
