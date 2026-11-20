using Competition.Application.Helpers;
using Competition.Domain.Entities;
using Competition.Infrastructure.Data;
using Competition.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using Competition.Application.Services;

namespace Competition.Tests.UnitTests.GalleryTests;

public class GalleryRepositoryReplaceTests
{
    private static IFormFile JpegFile()
    {
        var payload = new byte[] { 0xFF, 0xD8, 0xFF, 0x00, 0, 0, 0, 0, 0, 0, 0, 0 };
        return new FormFile(new MemoryStream(payload), 0, payload.Length, "picture", "photo.jpg")
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/jpeg"
        };
    }

    [Fact]
    public async Task UploadCountryPicture_DeletesPreviousBlobOnlyAfterSave()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(nameof(UploadCountryPicture_DeletesPreviousBlobOnlyAfterSave))
            .Options;
        await using var db = new AppDbContext(options);
        var countryId = Guid.NewGuid();
        const string oldUrl = "https://cdn.example.invalid/old.jpg";
        db.Countries.Add(new Country { Id = countryId, Name = "Testland", Slug = "testland", PictureUrl = oldUrl });
        await db.SaveChangesAsync();

        var deleted = new List<string>();
        var images = new Mock<IImageService>(MockBehavior.Strict);
        const string newUrl = "https://cdn.example.invalid/new.jpg";
        images.Setup(x => x.UploadImageAsync(It.IsAny<IFormFile>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(newUrl);
        images.Setup(x => x.DeleteImageAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback<string, CancellationToken>((url, _) => deleted.Add(url))
            .Returns(Task.CompletedTask);

        var repo = new GalleryRepository(db, images.Object);
        await repo.UploadAsync(GalleryRules.Country, countryId, JpegFile(), CancellationToken.None);

        var stored = await db.Countries.AsNoTracking().SingleAsync(c => c.Id == countryId);
        stored.PictureUrl.Should().Be(newUrl);
        deleted.Should().ContainSingle().Which.Should().Be(oldUrl);
        images.Verify(x => x.DeleteImageAsync(newUrl, It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UploadCountryPicture_WhenSaveFails_DeletesNewBlobAndKeepsOldReference()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(nameof(UploadCountryPicture_WhenSaveFails_DeletesNewBlobAndKeepsOldReference))
            .Options;
        await using var db = new FailingSaveDbContext(options);
        var countryId = Guid.NewGuid();
        const string oldUrl = "https://cdn.example.invalid/keep.jpg";
        db.Countries.Add(new Country { Id = countryId, Name = "Testland", Slug = "testland", PictureUrl = oldUrl });
        await db.SaveChangesAsync();

        var deleted = new List<string>();
        var images = new Mock<IImageService>(MockBehavior.Strict);
        const string newUrl = "https://cdn.example.invalid/orphan.jpg";
        images.Setup(x => x.UploadImageAsync(It.IsAny<IFormFile>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(newUrl);
        images.Setup(x => x.DeleteImageAsync(newUrl, It.IsAny<CancellationToken>()))
            .Callback<string, CancellationToken>((url, _) => deleted.Add(url))
            .Returns(Task.CompletedTask);

        var repo = new GalleryRepository(db, images.Object);
        var act = () => repo.UploadAsync(GalleryRules.Country, countryId, JpegFile(), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
        deleted.Should().ContainSingle().Which.Should().Be(newUrl);
        var stored = await db.Countries.AsNoTracking().SingleAsync(c => c.Id == countryId);
        stored.PictureUrl.Should().Be(oldUrl);
    }

    private sealed class FailingSaveDbContext : AppDbContext
    {
        private int _saves;

        public FailingSaveDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            if (++_saves > 1)
                throw new InvalidOperationException("Simulated persistence failure");

            return base.SaveChangesAsync(cancellationToken);
        }
    }
}