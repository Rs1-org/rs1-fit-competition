using Competition.Application.Common.Validation;
using Competition.Application.Interfaces;
using Competition.Domain.Entities;
using Competition.Infrastructure.Data;
using Competition.Infrastructure.Repositories;
using FluentAssertions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using CompetitionEntity = Competition.Domain.Entities.CompetitionEntities.Competition;

namespace Competition.Tests.UnitTests;

public class PublicIdTests
{
    private static AppDbContext NewContext() =>
        new(new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("PublicId_" + Guid.NewGuid())
            .Options);

    [Fact]
    public async Task SaveChanges_NewEntity_ShouldGenerateUniquePublicId()
    {
        // Arrange
        await using var context = NewContext();
        var first = new Country { Name = "Bosnia and Herzegovina", Slug = "bosnia-and-herzegovina" };
        var second = new Country { Name = "Croatia", Slug = "croatia" };
        context.Countries.AddRange(first, second);

        // Act
        await context.SaveChangesAsync();

        // Assert
        first.PublicId.Should().MatchRegex("^[0-9a-f]{32}$");
        second.PublicId.Should().MatchRegex("^[0-9a-f]{32}$");
        first.PublicId.Should().NotBe(second.PublicId);
    }

    [Fact]
    public async Task SaveChanges_PublicIdAlreadySet_ShouldKeepIt()
    {
        // Arrange
        await using var context = NewContext();
        var publicId = BaseEntity.NewPublicId();
        var country = new Country { Name = "Serbia", Slug = "serbia", PublicId = publicId };
        context.Countries.Add(country);

        // Act
        await context.SaveChangesAsync();
        country.Name = "Serbia (updated)";
        await context.SaveChangesAsync();

        // Assert
        country.PublicId.Should().Be(publicId);
    }

    [Fact]
    public async Task Lookup_KnownPublicId_ShouldReturnInternalId()
    {
        // Arrange
        await using var context = NewContext();
        var country = new Country { Name = "Slovenia", Slug = "slovenia" };
        context.Countries.Add(country);
        await context.SaveChangesAsync();
        var lookup = new PublicIdLookup(context);

        // Act
        var id = await lookup.GetIdByPublicIdAsync<Country>(country.PublicId);

        // Assert
        id.Should().Be(country.Id);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("00000000000000000000000000000000")]
    public async Task Lookup_UnknownPublicId_ShouldReturnNullAndRequiredShouldThrow404(string? publicId)
    {
        // Arrange
        await using var context = NewContext();
        var lookup = new PublicIdLookup(context);

        // Act
        var id = await lookup.GetIdByPublicIdAsync<Country>(publicId);
        var required = () => lookup.GetRequiredIdAsync<Country>(publicId);

        // Assert
        id.Should().BeNull();
        await required.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Lookup_SoftDeletedEntity_ShouldNotBeFound()
    {
        // Arrange
        await using var context = NewContext();
        var competition = new CompetitionEntity { Name = "Old Cup", Slug = "old-cup" };
        context.Competitions.Add(competition);
        await context.SaveChangesAsync();
        context.Competitions.Remove(competition); // SaveChanges turns this into a soft delete
        await context.SaveChangesAsync();
        var lookup = new PublicIdLookup(context);

        // Act
        var id = await lookup.GetIdByPublicIdAsync<CompetitionEntity>(competition.PublicId);

        // Assert
        competition.DeletedAt.Should().NotBeNull();
        id.Should().BeNull();
    }

    [Fact]
    public async Task Lookup_SoftDeletedEntityWithoutQueryFilter_ShouldNotBeFound()
    {
        // Arrange: Country has no HasQueryFilter, so the lookup itself must skip deleted rows.
        await using var context = NewContext();
        var country = new Country { Name = "Yugoslavia", Slug = "yugoslavia" };
        context.Countries.Add(country);
        await context.SaveChangesAsync();
        context.Countries.Remove(country);
        await context.SaveChangesAsync();
        var lookup = new PublicIdLookup(context);

        // Act
        var id = await lookup.GetIdByPublicIdAsync<Country>(country.PublicId);

        // Assert
        country.DeletedAt.Should().NotBeNull();
        id.Should().BeNull();
    }

    [Theory]
    [InlineData("3f2b9c1e8a7d4e6f9b0c1d2e3f4a5b6c", true)]
    [InlineData("3F2B9C1E8A7D4E6F9B0C1D2E3F4A5B6C", false)]
    [InlineData("3f2b9c1e-8a7d-4e6f-9b0c-1d2e3f4a5b6c", false)]
    [InlineData("1", false)]
    [InlineData(null, false)]
    public void MustBeValidPublicId_ShouldCheckFormat(string? publicId, bool expected)
    {
        var validator = new InlineValidator<Holder>();
        validator.RuleFor(x => x.PublicId).MustBeValidPublicId();

        validator.Validate(new Holder { PublicId = publicId }).IsValid.Should().Be(expected);
    }

    [Fact]
    public async Task MustReferenceExisting_UnknownPublicId_ShouldFailValidation()
    {
        // Arrange
        var lookup = new Mock<IPublicIdLookup>();
        lookup.Setup(l => l.GetIdByPublicIdAsync<Country>(It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid?)null);
        var validator = new InlineValidator<Holder>();
        validator.RuleFor(x => x.PublicId).MustReferenceExisting<Holder, Country>(lookup.Object);

        // Act
        var result = await validator.ValidateAsync(new Holder { PublicId = BaseEntity.NewPublicId() });

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.ErrorMessage == "Country with this public id does not exist.");
    }

    public class Holder
    {
        public string? PublicId { get; set; }
    }
}
