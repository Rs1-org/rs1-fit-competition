namespace Competition.Domain.Entities.FormationEntities;

public class FormationMark : BaseEntity
{
    public int Defenders { get; set; }
    public int Midfielders { get; set; }
    public int Attackers { get; set; }
    public string? Slug { get; set; }

    public Formation? Formation { get; set; }
}
