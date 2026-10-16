namespace Competition.Application.Common.Models;

public record CountryListFilter(
    string? Name,
    string? Slug,
    bool? HasPicture,
    DateTime? CreatedFrom,
    DateTime? CreatedTo,
    SortSpec? Sort = null);
