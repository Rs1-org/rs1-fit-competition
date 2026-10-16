namespace Competition.Application.DTOs.StadiumDto;

public class StadiumListDto
{
    public string PublicId { get; set; } = string.Empty;
    public string? Name { get; set; }
    public int Capacity { get; set; }
    public string? Slug { get; set; }
    public string? PictureUrl { get; set; }
    public string? CityName { get; set; }
    public string? CountryName { get; set; }
}
