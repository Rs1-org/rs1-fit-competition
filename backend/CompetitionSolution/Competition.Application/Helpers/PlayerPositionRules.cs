using Competition.Domain.Entities.FormationEntities;

namespace Competition.Application.Helpers;

public static class PlayerPositionRules
{
    public static void EnsureConsistent(Guid? positionId, Position? position, Guid? subPositionId, SubPosition? subPosition)
    {
        if (positionId.HasValue && position == null)
            throw new KeyNotFoundException($"Position '{positionId}' was not found.");

        if (!subPositionId.HasValue)
            return;

        if (subPosition == null)
            throw new KeyNotFoundException($"Subposition '{subPositionId}' was not found.");

        if (subPosition.PositionId != positionId)
            throw new InvalidOperationException("Subposition does not belong to the selected position.");
    }
}
