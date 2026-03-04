namespace Competition.Domain.Entities;

public class City : BaseEntity
{
            public string? Name { get; set; }
            public int Population {  get; set; }
            public bool IsCapital {  get; set; }
            public string? Slug { get; set; }
            
            // public List<Club>? Clubs {  get; set; }
            public required Guid GeoDataId { get; set; }
            public required Guid CountryId {  get; set; }
            
            public Country? Country { get; set; }
            public GeoData? GeoData { get; set; }
}