using Competition.Application.DTOs.GalleryDto;
using Competition.Application.Helpers;
using Competition.Application.Interfaces;
using Competition.Application.Services;
using Competition.Domain.Entities;
using Competition.Domain.Entities.MatchEntities;
using Competition.Domain.Entities.Person;
using Competition.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Competition.Infrastructure.Repositories;

public class GalleryRepository : IGallery
{
    private readonly AppDbContext _db;
    private readonly IImageService _images;

    public GalleryRepository(AppDbContext db, IImageService images)
    {
        _db = db;
        _images = images;
    }

    public async Task<IReadOnlyList<GallerySectionDto>> ListAsync(CancellationToken cancellationToken = default)
    {
        var players = await _db.Players.AsNoTracking()
            .Where(x => x.DeletedAt == null)
            .Select(x => new GalleryItemDto
            {
                Id = x.Id,
                Name = x.FirstName + " " + x.LastName,
                Kind = GalleryRules.Player,
                HasPicture = x.Picture.Length > 0
            })
            .ToListAsync(cancellationToken);

        var referees = await _db.Referees.AsNoTracking()
            .Where(x => x.DeletedAt == null)
            .Select(x => new GalleryItemDto
            {
                Id = x.Id,
                Name = x.FirstName + " " + x.LastName,
                Kind = GalleryRules.Referee,
                HasPicture = x.Picture.Length > 0
            })
            .ToListAsync(cancellationToken);

        var managers = await _db.Managers.AsNoTracking()
            .Where(x => x.DeletedAt == null)
            .Select(x => new GalleryItemDto
            {
                Id = x.Id,
                Name = x.FirstName + " " + x.LastName,
                Kind = GalleryRules.Manager,
                HasPicture = x.Picture.Length > 0
            })
            .ToListAsync(cancellationToken);

        var countries = await _db.Countries.AsNoTracking()
            .Where(x => x.DeletedAt == null)
            .Select(x => new GalleryItemDto
            {
                Id = x.Id,
                Name = x.Name ?? "Untitled",
                Kind = GalleryRules.Country,
                PictureUrl = x.PictureUrl,
                HasPicture = x.PictureUrl != null && x.PictureUrl != ""
            })
            .ToListAsync(cancellationToken);

        var stadiums = await _db.Stadiums.AsNoTracking()
            .Where(x => x.DeletedAt == null)
            .Select(x => new GalleryItemDto
            {
                Id = x.Id,
                Name = x.Name ?? "Untitled",
                Kind = GalleryRules.Stadium,
                PictureUrl = x.PictureUrl,
                HasPicture = x.PictureUrl != null && x.PictureUrl != ""
            })
            .ToListAsync(cancellationToken);

        var selections = await _db.Selections.AsNoTracking()
            .Select(x => new GalleryItemDto
            {
                Id = x.Id,
                Name = x.Name,
                Kind = GalleryRules.Selection,
                PictureUrl = x.PictureUrl,
                HasPicture = x.PictureUrl != null && x.PictureUrl != ""
            })
            .ToListAsync(cancellationToken);

        return
        [
            Section(GalleryRules.Player, players),
            Section(GalleryRules.Referee, referees),
            Section(GalleryRules.Manager, managers),
            Section(GalleryRules.Country, countries),
            Section(GalleryRules.Stadium, stadiums),
            Section(GalleryRules.Selection, selections)
        ];
    }

    public async Task<GalleryImageDto?> GetPersonImageAsync(string kind, Guid id, CancellationToken cancellationToken = default)
    {
        if (!GalleryRules.IsPerson(kind))
            return null;

        var bytes = kind switch
        {
            GalleryRules.Player => await PictureOf(_db.Players, id, cancellationToken),
            GalleryRules.Referee => await PictureOf(_db.Referees, id, cancellationToken),
            GalleryRules.Manager => await PictureOf(_db.Managers, id, cancellationToken),
            _ => null
        };

        if (bytes is null || bytes.Length == 0)
            return null;

        return new GalleryImageDto
        {
            Bytes = bytes,
            ContentType = GalleryRules.ContentTypeOf(bytes)
        };
    }

    public async Task<GalleryItemDto> UploadAsync(string kind, Guid id, IFormFile picture, CancellationToken cancellationToken = default)
    {
        return kind switch
        {
            GalleryRules.Player => await SavePerson(_db.Players, kind, id, picture, cancellationToken),
            GalleryRules.Referee => await SavePerson(_db.Referees, kind, id, picture, cancellationToken),
            GalleryRules.Manager => await SavePerson(_db.Managers, kind, id, picture, cancellationToken),
            GalleryRules.Country => await SaveCountry(id, picture, cancellationToken),
            GalleryRules.Stadium => await SaveStadium(id, picture, cancellationToken),
            GalleryRules.Selection => await SaveSelection(id, picture, cancellationToken),
            _ => throw new KeyNotFoundException("Unknown image type.")
        };
    }

    public async Task DeleteAsync(string kind, Guid id, CancellationToken cancellationToken = default)
    {
        switch (kind)
        {
            case GalleryRules.Player:
                await ClearPerson(_db.Players, id, cancellationToken);
                break;
            case GalleryRules.Referee:
                await ClearPerson(_db.Referees, id, cancellationToken);
                break;
            case GalleryRules.Manager:
                await ClearPerson(_db.Managers, id, cancellationToken);
                break;
            case GalleryRules.Country:
                await ClearUrl(await Find(_db.Countries, id, "Country", cancellationToken), x => x.PictureUrl, (x, value) => x.PictureUrl = value, cancellationToken);
                break;
            case GalleryRules.Stadium:
                await ClearUrl(await Find(_db.Stadiums, id, "Stadium", cancellationToken), x => x.PictureUrl, (x, value) => x.PictureUrl = value, cancellationToken);
                break;
            case GalleryRules.Selection:
                await ClearUrl(await Find(_db.Selections, id, "Selection", cancellationToken), x => x.PictureUrl, (x, value) => x.PictureUrl = value, cancellationToken);
                break;
            default:
                throw new KeyNotFoundException("Unknown image type.");
        }
    }

    private static GallerySectionDto Section(string kind, List<GalleryItemDto> items)
    {
        var ordered = items.OrderBy(x => x.Name).ToList();
        return new GallerySectionDto
        {
            Kind = kind,
            Title = GalleryRules.Title(kind),
            WithPicture = ordered.Where(x => x.HasPicture).ToList(),
            WithoutPicture = ordered.Where(x => !x.HasPicture).ToList()
        };
    }

    private static async Task<byte[]?> PictureOf<T>(DbSet<T> set, Guid id, CancellationToken cancellationToken) where T : Person
    {
        return await set.AsNoTracking()
            .Where(x => x.Id == id && x.DeletedAt == null)
            .Select(x => x.Picture)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private async Task<GalleryItemDto> SavePerson<T>(DbSet<T> set, string kind, Guid id, IFormFile picture, CancellationToken cancellationToken) where T : Person
    {
        var entity = await set.FirstOrDefaultAsync(x => x.Id == id && x.DeletedAt == null, cancellationToken)
            ?? throw new KeyNotFoundException("Record was not found.");

        await using var stream = picture.OpenReadStream();
        using var memory = new MemoryStream();
        await stream.CopyToAsync(memory, cancellationToken);
        entity.Picture = memory.ToArray();
        await _db.SaveChangesAsync(cancellationToken);

        return new GalleryItemDto
        {
            Id = entity.Id,
            Name = $"{entity.FirstName} {entity.LastName}".Trim(),
            Kind = kind,
            HasPicture = entity.Picture.Length > 0
        };
    }

    private async Task ClearPerson<T>(DbSet<T> set, Guid id, CancellationToken cancellationToken) where T : Person
    {
        var entity = await set.FirstOrDefaultAsync(x => x.Id == id && x.DeletedAt == null, cancellationToken)
            ?? throw new KeyNotFoundException("Record was not found.");
        entity.Picture = [];
        await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task<GalleryItemDto> SaveCountry(Guid id, IFormFile picture, CancellationToken cancellationToken)
    {
        var entity = await Find(_db.Countries, id, "Country", cancellationToken);
        entity.PictureUrl = await ReplaceUrl(entity.PictureUrl, picture);
        await _db.SaveChangesAsync(cancellationToken);
        return Item(entity.Id, entity.Name ?? "Untitled", GalleryRules.Country, entity.PictureUrl);
    }

    private async Task<GalleryItemDto> SaveStadium(Guid id, IFormFile picture, CancellationToken cancellationToken)
    {
        var entity = await Find(_db.Stadiums, id, "Stadium", cancellationToken);
        entity.PictureUrl = await ReplaceUrl(entity.PictureUrl, picture);
        await _db.SaveChangesAsync(cancellationToken);
        return Item(entity.Id, entity.Name ?? "Untitled", GalleryRules.Stadium, entity.PictureUrl);
    }

    private async Task<GalleryItemDto> SaveSelection(Guid id, IFormFile picture, CancellationToken cancellationToken)
    {
        var entity = await Find(_db.Selections, id, "Selection", cancellationToken);
        entity.PictureUrl = await ReplaceUrl(entity.PictureUrl, picture);
        await _db.SaveChangesAsync(cancellationToken);
        return Item(entity.Id, entity.Name, GalleryRules.Selection, entity.PictureUrl);
    }

    private async Task ClearUrl<T>(T entity, Func<T, string?> read, Action<T, string?> write, CancellationToken cancellationToken) where T : BaseEntity
    {
        var current = read(entity);
        write(entity, null);
        await _db.SaveChangesAsync(cancellationToken);
        await TryDelete(current);
    }

    private async Task<string> ReplaceUrl(string? current, IFormFile picture)
    {
        var uploaded = await _images.UploadImageAsync(picture);
        await TryDelete(current);
        return uploaded;
    }

    private async Task TryDelete(string? url)
    {
        if (string.IsNullOrWhiteSpace(url) || !Uri.TryCreate(url, UriKind.Absolute, out _))
            return;

        try
        {
            await _images.DeleteImageAsync(url);
        }
        catch
        {
            // The record already points at the new picture. A failed blob delete must not undo that.
        }
    }

    private static async Task<T> Find<T>(DbSet<T> set, Guid id, string label, CancellationToken cancellationToken) where T : BaseEntity
    {
        return await set.FirstOrDefaultAsync(x => x.Id == id && x.DeletedAt == null, cancellationToken)
            ?? throw new KeyNotFoundException($"{label} was not found.");
    }

    private static GalleryItemDto Item(Guid id, string name, string kind, string? pictureUrl) => new()
    {
        Id = id,
        Name = name,
        Kind = kind,
        PictureUrl = pictureUrl,
        HasPicture = !string.IsNullOrWhiteSpace(pictureUrl)
    };
}
