using Competition.Domain.Entities.Person;

namespace Competition.Domain.Entities.MatchEntities;

public class Event : BaseEntity
{
    public Guid MatchId { get; set; }
    public Match Match { get; set; } = null!;
    public Guid EventTypeId { get; set; }
    public EventType EventType { get; set; } = null!;
    public Guid MatchPartId { get; set; }
    public MatchPart MatchPart { get; set; } = null!;
    public Guid SelectionId { get; set; }
    public Selection Selection { get; set; } = null!;
    public Guid? PlayerId { get; set; }
    public Player? Player { get; set; }
    public Guid? RelatedPlayerId { get; set; }
    public Player? RelatedPlayer { get; set; }
    public int Minute { get; set; }
    public int AddedMinute { get; set; }
    public string? Note { get; set; }
}

public class EventType : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}

public class MatchPart : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}
