namespace Competition.Domain.Entities.CompetitionEntities;

public class Competition : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? ShortName { get; set; }
    public string? Description { get; set; }

    public Guid? CountryId { get; set; }
    public Country? Country { get; set; }

    public List<Season> Seasons { get; set; } = [];
}
