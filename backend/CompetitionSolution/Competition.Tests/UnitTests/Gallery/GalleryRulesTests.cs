using Competition.Application.Helpers;
using FluentAssertions;
using Xunit;

namespace Competition.Tests.UnitTests.GalleryTests;

public class GalleryRulesTests
{
    [Fact]
    public void Title_NamesEveryPictureOwner()
    {
        GalleryRules.Title(GalleryRules.Player).Should().Be("Igrači");
        GalleryRules.Title(GalleryRules.Referee).Should().Be("Sudije");
        GalleryRules.Title(GalleryRules.Manager).Should().Be("Menadžeri");
        GalleryRules.Title(GalleryRules.Country).Should().Be("Države");
        GalleryRules.Title(GalleryRules.Stadium).Should().Be("Stadioni");
        GalleryRules.Title(GalleryRules.Selection).Should().Be("Reprezentacije");
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
}
