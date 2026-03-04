namespace Competition.Domain.Entities;

public class Country : BaseEntity
{
    public string? Name { get; set; }
    public string? Slug {  get; set; }
    public List<City>? Cities { get; set; }
    //public List<Competition>? Competitions { get; set; }
    //public List<Person>? Persons { get; set; }
    public string? PictureUrl { get; set; }
}