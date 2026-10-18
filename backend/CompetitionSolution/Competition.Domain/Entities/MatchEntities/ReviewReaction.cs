namespace Competition.Domain.Entities.MatchEntities;

public class ReviewReaction : BaseEntity
{
    public Guid ReviewId { get; set; }
    public MatchReview Review { get; set; } = null!;
    public Guid UserId { get; set; }
    public int Value { get; set; }
}
