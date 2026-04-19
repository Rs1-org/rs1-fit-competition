using System.ComponentModel.DataAnnotations.Schema;

namespace Competition.Domain.Entities;

public class GeoData:BaseEntity
{
    [Column(TypeName = "decimal(9,6)")]
    public decimal Latitude { get; set; }

    [Column(TypeName = "decimal(9,6)")]
    public decimal Longitude { get; set; }
    public string? Description {  get; set; }
}