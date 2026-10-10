namespace Competition.Domain.Entities.FormationEntities;

public class Formation : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string? Slug { get; set; }

    public Guid FormationMarkId { get; set; }
    public FormationMark? FormationMark { get; set; }
}
