namespace Competition.Application.DTOs.MatchDto;

public class MatchStatusListDto
{
    public string PublicId { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? Slug { get; set; }
    public int SortOrder { get; set; }
}
