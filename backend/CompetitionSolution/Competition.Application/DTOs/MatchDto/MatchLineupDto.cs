namespace Competition.Application.DTOs.MatchDto;

public class MatchLineupDto
{
    public string MatchPublicId { get; set; } = string.Empty;
    public string? RefereePublicId { get; set; }
    public string? RefereeName { get; set; }
    public string? HomeFormationPublicId { get; set; }
    public string? HomeFormationCode { get; set; }
    public string? AwayFormationPublicId { get; set; }
    public string? AwayFormationCode { get; set; }
    public List<LineupPlayerDto> Home { get; set; } = [];
    public List<LineupPlayerDto> Away { get; set; } = [];
}

public class LineupPlayerDto
{
    public string PublicId { get; set; } = string.Empty;
    public string PlayerPublicId { get; set; } = string.Empty;
    public string PlayerName { get; set; } = string.Empty;
    public string SelectionPublicId { get; set; } = string.Empty;
    public string? PositionPublicId { get; set; }
    public string? PositionName { get; set; }
    public string? SubPositionPublicId { get; set; }
    public string? SubPositionName { get; set; }
    public bool IsStarter { get; set; }
    public int? ShirtNumber { get; set; }
}

public class SaveLineupPlayerDto
{
    public string PlayerPublicId { get; set; } = string.Empty;
    public string SelectionPublicId { get; set; } = string.Empty;
    public string? PositionPublicId { get; set; }
    public string? SubPositionPublicId { get; set; }
    public bool IsStarter { get; set; }
    public int? ShirtNumber { get; set; }
}
