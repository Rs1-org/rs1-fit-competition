namespace Competition.Application.DTOs.StatisticDto;

public record RoundGoalsDto(int RoundNumber, string RoundName, int Matches, int Goals);

public record TeamResultsDto(string SelectionPublicId, string SelectionName,
    int Played, int Wins, int Draws, int Losses, int GoalsFor, int GoalsAgainst, int Points);

public record TeamShotsDto(string SelectionPublicId, string SelectionName,
    int Matches, double AverageShots, double AverageShotsOnTarget, double AveragePossession);

public class StatisticChartsDto
{
    public IReadOnlyList<RoundGoalsDto> GoalsPerRound { get; init; } = [];
    public IReadOnlyList<TeamResultsDto> TeamResults { get; init; } = [];
    public IReadOnlyList<TeamShotsDto> TeamShots { get; init; } = [];
}
