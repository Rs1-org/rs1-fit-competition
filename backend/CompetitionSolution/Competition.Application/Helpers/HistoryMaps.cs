using Competition.Application.DTOs.HistoryDto;
using Competition.Domain.Entities;

namespace Competition.Application.Helpers;

public static class HistoryMaps
{
    public static TransferValueHistoryDto ToDto(TransferValueHistory entity) => new()
    {
        PublicId = entity.PublicId,
        Date = NodaTimeMappingHelper.FormatNullableLocalDate(entity.Date),
        Value = entity.Value,
        PlayerPublicId = entity.Player?.PublicId ?? string.Empty
    };

    public static PlayerSelectionHistoryDto ToDto(PlayerSelectionHistory entity) => new()
    {
        PublicId = entity.PublicId,
        Goals = entity.Goals,
        MatchPlayed = entity.MatchPlayed,
        Assists = entity.Assists,
        ShirtNumber = entity.ShirtNumber,
        PlayerPublicId = entity.Player?.PublicId ?? string.Empty,
        SelectionPublicId = entity.Selection?.PublicId
    };

    public static SelectionManagerHistoryDto ToDto(SelectionManagerHistory entity) => new()
    {
        PublicId = entity.PublicId,
        Notes = entity.Notes,
        Wins = entity.Wins,
        Loses = entity.Loses,
        Draw = entity.Draw,
        ManagerPublicId = entity.Manager?.PublicId ?? string.Empty,
        SelectionPublicId = entity.Selection?.PublicId
    };

    public static MandateDateDto ToDto(MandateDate entity) => new()
    {
        PublicId = entity.PublicId,
        StartDate = NodaTimeMappingHelper.FormatNullableLocalDate(entity.StartDate),
        EndDate = NodaTimeMappingHelper.FormatNullableLocalDate(entity.EndDate),
        PlayerSelectionHistoryPublicId = entity.PlayerSelectionHistory?.PublicId,
        SelectionManagerHistoryPublicId = entity.SelectionManagerHistory?.PublicId
    };
}
