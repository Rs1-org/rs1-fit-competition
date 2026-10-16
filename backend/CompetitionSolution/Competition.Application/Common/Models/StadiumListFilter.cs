namespace Competition.Application.Common.Models;

public record StadiumListFilter(
    string? Name,
    Guid? CountryId,
    Guid? CityId,
    int? MinCapacity,
    int? MaxCapacity,
    SortSpec? Sort = null);
