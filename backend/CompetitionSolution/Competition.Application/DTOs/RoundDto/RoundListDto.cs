namespace Competition.Application.DTOs.RoundDto;

public class RoundListDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public int Number { get; set; }
    public int MatchCount { get; set; }
}
