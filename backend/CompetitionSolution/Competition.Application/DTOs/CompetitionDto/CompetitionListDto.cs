namespace Competition.Application.DTOs.CompetitionDto;

public class CompetitionListDto
{
    public string PublicId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? ShortName { get; set; }
    public string? CountryName { get; set; }
    public int SeasonCount { get; set; }
}
