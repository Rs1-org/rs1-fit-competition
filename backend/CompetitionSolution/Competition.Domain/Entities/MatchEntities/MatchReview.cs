namespace Competition.Domain.Entities.MatchEntities;

public class MatchReview : BaseEntity
{
    public Guid MatchId { get; set; }
    public Match Match { get; set; } = null!;
    public Guid AuthorId { get; set; }
    public ApplicationUser Author { get; set; } = null!;
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public ICollection<ReviewReaction> Reactions { get; set; } = [];
}
