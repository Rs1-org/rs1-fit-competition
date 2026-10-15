using Competition.Application.DTOs.HistoryDto;
using Competition.Domain.Entities;

namespace Competition.Application.Helpers;

public static class HistoryMaps
{
    public static TransferValueHistoryDto ToDto(TransferValueHistory entity) => new()
    {
        Id = entity.Id,
        Date = NodaTimeMappingHelper.FormatNullableLocalDate(entity.Date),
        Value = entity.Value,
        PlayerId = entity.PlayerId
    };

    public static PlayerSelectionHistoryDto ToDto(PlayerSelectionHistory entity) => new()
    {
        Id = entity.Id,
        Goals = entity.Goals,
        MatchPlayed = entity.MatchPlayed,
        Assists = entity.Assists,
        ShirtNumber = entity.ShirtNumber,
        PlayerId = entity.PlayerId,
        SelectionId = entity.SelectionId
    };

    public static SelectionManagerHistoryDto ToDto(SelectionManagerHistory entity) => new()
    {
        Id = entity.Id,
        Notes = entity.Notes,
        Wins = entity.Wins,
        Loses = entity.Loses,
        Draw = entity.Draw,
        ManagerId = entity.ManagerId,
        SelectionId = entity.SelectionId
    };

    public static MandateDateDto ToDto(MandateDate entity) => new()
    {
        Id = entity.Id,
        StartDate = NodaTimeMappingHelper.FormatNullableLocalDate(entity.StartDate),
        EndDate = NodaTimeMappingHelper.FormatNullableLocalDate(entity.EndDate),
        PlayerSelectionHistoryId = entity.PlayerSelectionHistoryId,
        SelectionManagerHistoryId = entity.SelectionManagerHistoryId
    };
}
