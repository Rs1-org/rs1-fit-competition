using Competition.Application.Commands.SelectionCommands.Create;
using Competition.Application.Commands.SelectionCommands.Update;
using Competition.Application.Interfaces;
using Competition.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;
using SelectionEntity = Competition.Domain.Entities.Selection;

namespace Competition.Tests.UnitTests.SelectionPublicId;

public class SelectionCountryPublicIdTests
{
    private readonly Mock<ISelection> _selections = new();
    private readonly Mock<IPublicIdLookup> _publicIds = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    [Fact]
    public async Task Create_KnownCountryPublicId_ShouldStoreInternalIdAndReturnPublicId()
    {
        var countryId = Guid.NewGuid();
        var publicId = BaseEntity.NewPublicId();
        var country = new Country { Id = countryId, Name = "Bosnia and Herzegovina", PublicId = publicId, Slug = "bosnia" };

        _publicIds
            .Setup(l => l.GetRequiredIdAsync<Country>(publicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(countryId);
        _selections
            .SetupSequence(s => s.GetBySlugAsync("bosnia", It.IsAny<CancellationToken>()))
            .ReturnsAsync((SelectionEntity?)null)
            .ReturnsAsync(new SelectionEntity
            {
                Name = "Bosnia",
                Abbreviation = "BIH",
                Slug = "bosnia",
                CountryId = countryId,
                Country = country,
                FollowersCount = 10
            });

        var handler = new CreateSelectionCommandHandler(_selections.Object, _publicIds.Object, _unitOfWork.Object);

        var result = await handler.Handle(new CreateSelectionCommand
        {
            Name = "Bosnia",
            Abbreviation = "bih",
            FollowersCount = 10,
            CountryPublicId = publicId
        }, CancellationToken.None);

        result.CountryPublicId.Should().Be(publicId);
        result.CountryName.Should().Be("Bosnia and Herzegovina");
        _selections.Verify(s => s.AddAsync(It.Is<SelectionEntity>(x => x.CountryId == countryId), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Create_UnknownCountryPublicId_ShouldThrowNotFound()
    {
        var publicId = BaseEntity.NewPublicId();
        _publicIds
            .Setup(l => l.GetRequiredIdAsync<Country>(publicId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException($"Country with public id '{publicId}' not found."));

        var handler = new CreateSelectionCommandHandler(_selections.Object, _publicIds.Object, _unitOfWork.Object);

        var act = () => handler.Handle(new CreateSelectionCommand
        {
            Name = "Bosnia",
            Abbreviation = "BIH",
            CountryPublicId = publicId
        }, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
        _selections.Verify(s => s.AddAsync(It.IsAny<SelectionEntity>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Update_KnownCountryPublicId_ShouldReplaceCountry()
    {
        var countryId = Guid.NewGuid();
        var publicId = BaseEntity.NewPublicId();
        var existing = new SelectionEntity
        {
            Id = Guid.NewGuid(),
            Name = "Old",
            Abbreviation = "OLD",
            Slug = "old",
            CountryId = Guid.NewGuid()
        };
        var country = new Country { Id = countryId, Name = "Croatia", PublicId = publicId, Slug = "croatia" };

        _selections
            .Setup(s => s.GetBySlugAsync("old", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);
        _selections
            .Setup(s => s.GetBySlugAsync("croatia", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SelectionEntity
            {
                Id = existing.Id,
                Name = "Croatia",
                Abbreviation = "CRO",
                Slug = "croatia",
                CountryId = countryId,
                Country = country
            });
        _publicIds
            .Setup(l => l.GetRequiredIdAsync<Country>(publicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(countryId);

        var handler = new UpdateSelectionCommandHandler(_selections.Object, _publicIds.Object, _unitOfWork.Object);

        var result = await handler.Handle(new UpdateSelectionCommand
        {
            Slug = "old",
            Name = "Croatia",
            Abbreviation = "cro",
            CountryPublicId = publicId
        }, CancellationToken.None);

        result.CountryPublicId.Should().Be(publicId);
        existing.CountryId.Should().Be(countryId);
    }

    [Fact]
    public async Task Validator_UnknownCountryPublicId_ShouldFail()
    {
        var lookup = new Mock<IPublicIdLookup>();
        lookup.Setup(l => l.GetIdByPublicIdAsync<Country>(It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid?)null);

        var validator = new CreateSelectionCommandValidator(lookup.Object);
        var result = await validator.ValidateAsync(new CreateSelectionCommand
        {
            Name = "Bosnia",
            Abbreviation = "BIH",
            CountryPublicId = BaseEntity.NewPublicId()
        });

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateSelectionCommand.CountryPublicId));
    }
}
