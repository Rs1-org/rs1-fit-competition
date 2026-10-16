namespace Competition.Application.DTOs.RoundDto;

public class RoundDetailsDto
{
    public string PublicId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public int Number { get; set; }
    public string SeasonPublicId { get; set; } = string.Empty;
    public string SeasonName { get; set; } = string.Empty;
    public string SeasonSlug { get; set; } = string.Empty;
    public string CompetitionName { get; set; } = string.Empty;
    public string CompetitionSlug { get; set; } = string.Empty;
}
