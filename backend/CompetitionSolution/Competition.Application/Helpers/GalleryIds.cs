using Competition.Application.Interfaces;
using Competition.Domain.Entities;
using Competition.Domain.Entities.MatchEntities;
using Competition.Domain.Entities.Person;

namespace Competition.Application.Helpers;

public static class GalleryIds
{
    public static Task<Guid> ResolveAsync(IPublicIdLookup lookup, string kind, string publicId, CancellationToken cancellationToken) =>
        kind switch
        {
            GalleryRules.Player => lookup.GetRequiredIdAsync<Player>(publicId, cancellationToken),
            GalleryRules.Referee => lookup.GetRequiredIdAsync<Referee>(publicId, cancellationToken),
            GalleryRules.Manager => lookup.GetRequiredIdAsync<Manager>(publicId, cancellationToken),
            GalleryRules.Country => lookup.GetRequiredIdAsync<Country>(publicId, cancellationToken),
            GalleryRules.Stadium => lookup.GetRequiredIdAsync<Stadium>(publicId, cancellationToken),
            GalleryRules.Selection => lookup.GetRequiredIdAsync<Selection>(publicId, cancellationToken),
            _ => throw new KeyNotFoundException("Unknown image type.")
        };
}
