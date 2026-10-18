namespace Competition.Application.DTOs.StatisticDto;

public record TeamStatisticValues(int TotalShots, int ShotsOnTarget, int TotalRedCards,
    int TotalYellowCards, int Corners, int BigChances, int Possession);

public class TeamStatisticDto
{
    public string PublicId { get; set; } = string.Empty;
    public string SelectionPublicId { get; set; } = string.Empty;
    public string SelectionName { get; set; } = string.Empty;
    public int TotalShots { get; set; }
    public int ShotsOnTarget { get; set; }
    public int TotalRedCards { get; set; }
    public int TotalYellowCards { get; set; }
    public int Corners { get; set; }
    public int BigChances { get; set; }
    public int Possession { get; set; }
}

public class StatisticDto
{
    public string PublicId { get; set; } = string.Empty;
    public string MatchPublicId { get; set; } = string.Empty;
    public string? MatchSlug { get; set; }
    public string MatchDate { get; set; } = string.Empty;
    public TeamStatisticDto? Home { get; set; }
    public TeamStatisticDto? Away { get; set; }
}

public record DeletedStatisticDto(string PublicId);
