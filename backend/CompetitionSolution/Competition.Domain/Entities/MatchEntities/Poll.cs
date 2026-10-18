using NodaTime;

namespace Competition.Domain.Entities.MatchEntities;

public class Poll : BaseEntity
{
    public Guid MatchId { get; set; }
    public Match Match { get; set; } = null!;
    public string Question { get; set; } = string.Empty;
    public Instant OpensAt { get; set; }
    public Instant ClosesAt { get; set; }
    public ICollection<PollOption> Options { get; set; } = [];
    public ICollection<PollVote> Votes { get; set; } = [];
}
public class PollOption : BaseEntity
{
    public Guid PollId { get; set; }
    public Poll Poll { get; set; } = null!;
    public string Label { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public ICollection<PollVote> Votes { get; set; } = [];
}
public class PollVote : BaseEntity
{
    public Guid PollId { get; set; }
    public Poll Poll { get; set; } = null!;
    public Guid OptionId { get; set; }
    public PollOption Option { get; set; } = null!;
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;
}
