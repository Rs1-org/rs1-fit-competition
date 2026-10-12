using NodaTime;
using Competition.Domain.Entities;
using Competition.Domain.Entities.CompetitionEntities;

namespace Competition.Domain.Entities.MatchEntities;

public class Match : BaseEntity
{
    public Instant Date { get; set; }
    public int FollowersCount { get; set; }
    public int? HomeScore { get; set; }
    public int? AwayScore { get; set; }
    public int? HalfHomeScore { get; set; }
    public int? HalfAwayScore { get; set; }
    public bool IsPostponed { get; set; }
    public string? Slug { get; set; }

    public Guid StatusId { get; set; }
    public MatchStatus? Status { get; set; }

    public Guid StadiumId { get; set; }
    public Stadium? Stadium { get; set; }

    public Guid? HomeSelectionId { get; set; }
    public Selection? HomeSelection { get; set; }

    public Guid? AwaySelectionId { get; set; }
    public Selection? AwaySelection { get; set; }

    public Guid? RoundId { get; set; }
    public Round? Round { get; set; }
}
