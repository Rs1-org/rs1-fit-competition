using Competition.Application.Commands.GalleryCommands.Delete;
using Competition.Application.Commands.GalleryCommands.Upload;
using Competition.Application.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace Competition.Tests.UnitTests.GalleryTests;

public class GalleryCommandValidatorTests
{
    private readonly UploadGalleryImageCommandValidator _upload = new();
    private readonly DeleteGalleryImageCommandValidator _delete = new();

    [Fact]
    public void Upload_RejectsMissingFileWithEnglishMessage()
    {
        var result = _upload.Validate(new UploadGalleryImageCommand
        {
            Kind = GalleryRules.Player,
            PublicId = Guid.NewGuid().ToString("N"),
            Picture = null
        });

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.ErrorMessage == "Choose an image.");
    }

    [Fact]
    public void Upload_RejectsOversizedFileWithEnglishMessage()
    {
        var result = _upload.Validate(new UploadGalleryImageCommand
        {
            Kind = GalleryRules.Player,
            PublicId = Guid.NewGuid().ToString("N"),
            Picture = FileOf("image/jpeg", (int)GalleryRules.MaxBytes + 1)
        });

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.ErrorMessage == "File is too large.");
    }

    [Fact]
    public void Upload_RejectsUnknownKindAndEmptyId()
    {
        var result = _upload.Validate(new UploadGalleryImageCommand
        {
            Kind = "club",
            PublicId = "",
            Picture = FileOf("image/png", 32)
        });

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == "Kind" && error.ErrorMessage == "Unknown image type.");
        result.Errors.Should().Contain(error => error.PropertyName == "PublicId");
    }

    [Fact]
    public void Upload_AcceptsJpegForAKnownOwner()
    {
        var result = _upload.Validate(new UploadGalleryImageCommand
        {
            Kind = GalleryRules.Stadium,
            PublicId = Guid.NewGuid().ToString("N"),
            Picture = FileOf("image/jpeg", 128)
        });

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Delete_RejectsEmptyIdAndUnknownKind()
    {
        var result = _delete.Validate(new DeleteGalleryImageCommand { Kind = "club", PublicId = "" });

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == "PublicId");
        result.Errors.Should().Contain(error => error.ErrorMessage == "Unknown image type.");
    }

    [Fact]
    public void Delete_AcceptsKnownKindAndId()
    {
        var result = _delete.Validate(new DeleteGalleryImageCommand
        {
            Kind = GalleryRules.Selection,
            PublicId = Guid.NewGuid().ToString("N")
        });

        result.IsValid.Should().BeTrue();
    }

    private static FormFile FileOf(string contentType, int length)
    {
        var size = Math.Max(length, 12);
        var payload = new byte[size];
        var type = contentType.ToLowerInvariant();
        if (type is "image/jpeg" or "image/jpg")
        {
            payload[0] = 0xFF;
            payload[1] = 0xD8;
            payload[2] = 0xFF;
        }
        else if (type == "image/png")
        {
            payload[0] = 0x89;
            payload[1] = 0x50;
            payload[2] = 0x4E;
            payload[3] = 0x47;
        }
        else if (type == "image/webp")
        {
            payload[0] = (byte)'R';
            payload[1] = (byte)'I';
            payload[2] = (byte)'F';
            payload[3] = (byte)'F';
            payload[8] = (byte)'W';
            payload[9] = (byte)'E';
            payload[10] = (byte)'B';
            payload[11] = (byte)'P';
        }

        return new FormFile(new MemoryStream(payload), 0, size, "picture", "photo.jpg")
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };
    }
}
