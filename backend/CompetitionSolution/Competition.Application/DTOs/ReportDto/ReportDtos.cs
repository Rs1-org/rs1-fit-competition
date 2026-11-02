using Competition.Application.DTOs.MatchDto;
using Competition.Application.Events;

namespace Competition.Application.DTOs.ReportDto;

/// <summary>Everything printed on the single match report (RS1 item 12).</summary>
public class MatchReportDto
{
    public string HomeName { get; set; } = string.Empty;
    public string AwayName { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string? StatusName { get; set; }
    public bool IsPostponed { get; set; }
    public string? StadiumName { get; set; }
    public string? CompetitionName { get; set; }
    public string? RoundName { get; set; }
    public int? HomeScore { get; set; }
    public int? AwayScore { get; set; }
    public int? HalfHomeScore { get; set; }
    public int? HalfAwayScore { get; set; }
    public MatchLineupDto Lineup { get; set; } = new();
    public IReadOnlyList<EventDto> Events { get; set; } = [];
    public IReadOnlyList<TeamStatisticReportDto> Statistics { get; set; } = [];
}

public class TeamStatisticReportDto
{
    public string SelectionName { get; set; } = string.Empty;
    public int TotalShots { get; set; }
    public int ShotsOnTarget { get; set; }
    public int Possession { get; set; }
    public int Corners { get; set; }
    public int BigChances { get; set; }
    public int TotalYellowCards { get; set; }
    public int TotalRedCards { get; set; }
}

/// <summary>Everything printed on the matches by date range report (RS1 item 12).</summary>
public class MatchesReportDto
{
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
    public IReadOnlyList<string> Filters { get; set; } = [];
    public IReadOnlyList<MatchesReportRowDto> Rows { get; set; } = [];
    /// <summary>All matching matches; higher than <see cref="Rows"/> when the report was cut off.</summary>
    public int TotalCount { get; set; }
}

public class MatchesReportRowDto
{
    public DateTime Date { get; set; }
    public string HomeName { get; set; } = string.Empty;
    public string AwayName { get; set; } = string.Empty;
    public int? HomeScore { get; set; }
    public int? AwayScore { get; set; }
    public string? StatusName { get; set; }
    public bool IsPostponed { get; set; }
    public string? StadiumName { get; set; }
    public string? CompetitionName { get; set; }
}
