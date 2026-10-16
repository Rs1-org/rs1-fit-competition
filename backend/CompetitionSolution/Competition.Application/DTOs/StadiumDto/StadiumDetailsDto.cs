namespace Competition.Application.DTOs.StadiumDto;

public class StadiumDetailsDto
{
    public string PublicId { get; set; } = string.Empty;
    public string? Name { get; set; }
    public int Capacity { get; set; }
    public DateTime DateOfConstruction { get; set; }
    public string? Slug { get; set; }
    public string? PictureUrl { get; set; }
    public string CityPublicId { get; set; } = string.Empty;
    public string? CityName { get; set; }
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public string? GeoDescription { get; set; }
    public decimal? CityLatitude { get; set; }
    public decimal? CityLongitude { get; set; }
}
