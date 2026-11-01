using Competition.Domain.Entities.FormationEntities;
using Competition.Domain.Entities.Person;

namespace Competition.Domain.Entities.MatchEntities;

public class MatchLineup : BaseEntity
{
    public Guid MatchId { get; set; }
    public Match? Match { get; set; }

    public Guid SelectionId { get; set; }
    public Selection? Selection { get; set; }

    public Guid PlayerId { get; set; }
    public Player? Player { get; set; }

    public Guid? PositionId { get; set; }
    public Position? Position { get; set; }

    public Guid? SubPositionId { get; set; }
    public SubPosition? SubPosition { get; set; }

    public bool IsStarter { get; set; }
    public int? ShirtNumber { get; set; }
}
