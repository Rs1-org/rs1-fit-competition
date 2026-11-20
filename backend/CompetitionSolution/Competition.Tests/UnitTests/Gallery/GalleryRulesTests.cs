using Competition.Application.Helpers;
using FluentAssertions;
using FluentValidation;
using Xunit;

namespace Competition.Tests.UnitTests.GalleryTests;

public class GalleryRulesTests
{
    [Fact]
    public void Title_NamesEveryPictureOwner()
    {
        GalleryRules.Title(GalleryRules.Player).Should().Be("Players");
        GalleryRules.Title(GalleryRules.Referee).Should().Be("Referees");
        GalleryRules.Title(GalleryRules.Manager).Should().Be("Managers");
        GalleryRules.Title(GalleryRules.Country).Should().Be("Countries");
        GalleryRules.Title(GalleryRules.Stadium).Should().Be("Stadiums");
        GalleryRules.Title(GalleryRules.Selection).Should().Be("Selections");
    }

    [Fact]
    public void IsPerson_CoversOnlyStoredPortraits()
    {
        GalleryRules.IsPerson(GalleryRules.Player).Should().BeTrue();
        GalleryRules.IsPerson(GalleryRules.Referee).Should().BeTrue();
        GalleryRules.IsPerson(GalleryRules.Manager).Should().BeTrue();
        GalleryRules.IsPerson(GalleryRules.Country).Should().BeFalse();
    }

    [Fact]
    public void ContentTypeOf_DetectsJpegAndPng()
    {
        GalleryRules.ContentTypeOf([0xFF, 0xD8, 0xFF, 0x00]).Should().Be("image/jpeg");
        GalleryRules.ContentTypeOf([0x89, 0x50, 0x4E, 0x47]).Should().Be("image/png");
    }

    [Fact]
    public void EnsurePictureBytes_AcceptsJpegDeclaredAsImageJpg()
    {
        var jpeg = new byte[] { 0xFF, 0xD8, 0xFF, 0x00 };
        var act = () => GalleryRules.EnsurePictureBytes(jpeg, "image/jpg");
        act.Should().NotThrow();
    }

    [Fact]
    public void EnsurePictureBytes_RejectsMismatchedContent()
    {
        var jpeg = new byte[] { 0xFF, 0xD8, 0xFF, 0x00 };
        var act = () => GalleryRules.EnsurePictureBytes(jpeg, "image/png");
        act.Should().Throw<ValidationException>()
            .Which.Errors.Should().Contain(e => e.PropertyName == "picture");
    }

    [Fact]
    public void NormalizeDeclaredContentType_MapsJpgToJpeg()
    {
        GalleryRules.NormalizeDeclaredContentType("image/jpg").Should().Be("image/jpeg");
        GalleryRules.NormalizeDeclaredContentType("image/jpeg").Should().Be("image/jpeg");
    }
}
