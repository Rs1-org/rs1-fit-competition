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

    private static FormFile FileOf(string contentType, int length) =>
        new(new MemoryStream(new byte[Math.Max(length, 0)]), 0, length, "picture", "photo.jpg")
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };
}
