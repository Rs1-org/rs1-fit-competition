namespace Competition.Application.DTOs.CompetitionDto;

public class CompetitionDetailsDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? ShortName { get; set; }
    public string? Description { get; set; }
    public Guid? CountryId { get; set; }
    public string? CountryName { get; set; }
}
