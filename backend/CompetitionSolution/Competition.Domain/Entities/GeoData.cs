namespace Competition.Domain.Entities;

public class GeoData:BaseEntity
{
    public decimal Latitude { get; set; }

    public decimal Longitude { get; set; }
    public string? Description {  get; set; }
}