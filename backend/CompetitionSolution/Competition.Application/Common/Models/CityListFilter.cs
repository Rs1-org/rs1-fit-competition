namespace Competition.Application.Common.Models;

public record CityListFilter(
    string? Name,
    Guid? CountryId,
    bool? IsCapital,
    int? MinPopulation,
    int? MaxPopulation,
    SortSpec? Sort = null);
