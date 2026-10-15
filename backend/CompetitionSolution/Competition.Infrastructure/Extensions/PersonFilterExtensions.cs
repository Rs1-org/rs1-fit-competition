using NodaTime;
using PersonEntity = Competition.Domain.Entities.Person.Person;

namespace Competition.Infrastructure.Extensions;

public static class PersonFilterExtensions
{
    public static IQueryable<TPerson> FilterByName<TPerson>(this IQueryable<TPerson> query, string? name)
        where TPerson : PersonEntity
    {
        if (string.IsNullOrWhiteSpace(name))
            return query;

        var term = name.Trim().ToLower();
        return query.Where(p => p.FirstName.ToLower().Contains(term) || p.LastName.ToLower().Contains(term));
    }

    public static IQueryable<TPerson> FilterByAge<TPerson>(this IQueryable<TPerson> query, int? ageMin, int? ageMax)
        where TPerson : PersonEntity
    {
        var today = LocalDate.FromDateTime(DateTime.UtcNow);
        if (ageMin is int min)
        {
            var bornOnOrBefore = today.PlusYears(-min);
            query = query.Where(p => p.DateOfBirth <= bornOnOrBefore);
        }

        if (ageMax is int max)
        {
            var bornOnOrAfter = today.PlusYears(-(max + 1)).PlusDays(1);
            query = query.Where(p => p.DateOfBirth >= bornOnOrAfter);
        }

        return query;
    }
}
