using Competition.Application.Commands.SelectionCommands.Create;
using Competition.Application.Commands.SelectionCommands.Update;
using Competition.Application.Interfaces;
using Competition.Application.Queries.SelectionQueries.GetAll;
using Competition.Application.Queries.SelectionQueries.GetBySlug;
using Competition.Domain.Entities;
using Competition.Infrastructure.Data;
using Competition.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;
using SelectionEntity = Competition.Domain.Entities.Selection;

namespace Competition.Tests.UnitTests.SelectionPublicId;

public class SelectionClubTests
{
    private static AppDbContext NewContext() => new(new DbContextOptionsBuilder<AppDbContext>()
        .UseInMemoryDatabase("SelectionClub_" + Guid.NewGuid()).Options);

    [Fact]
    public async Task CreateAndUpdate_ResolveClubPublicId_AndReturnClubDetails()
    {
        await using var db = NewContext();
        var country = new Country { Name = "Country", Slug = "country" };
        var first = new Club { Name = "First", Slug = "first" };
        var second = new Club { Name = "Second", Slug = "second" };
        db.AddRange(country, first, second);
        await db.SaveChangesAsync();
        var repo = new SelectionRepository(db);
        var lookup = new PublicIdLookup(db);
        var unit = new UnitOfWork(db);
        using var cts = new CancellationTokenSource();
        var token = cts.Token;
        var request = new CreateSelectionCommand
        {
            Name = "Youth", Abbreviation = "YTH", CountryPublicId = country.PublicId, ClubPublicId = first.PublicId
        };
        (await new CreateSelectionCommandValidator(lookup).ValidateAsync(request, token)).IsValid.Should().BeTrue();
        var created = await new CreateSelectionCommandHandler(repo, lookup, unit).Handle(request, token);
        created.ClubPublicId.Should().Be(first.PublicId);
        created.ClubName.Should().Be("First");
        created.ClubSlug.Should().Be("first");
        (await db.Selections.SingleAsync(token)).ClubId.Should().Be(first.Id);

        var update = new UpdateSelectionCommand
        {
            Slug = "youth", Name = "Youth", Abbreviation = "YTH", CountryPublicId = country.PublicId, ClubPublicId = second.PublicId
        };
        (await new UpdateSelectionCommandValidator(lookup).ValidateAsync(update, token)).IsValid.Should().BeTrue();
        var updated = await new UpdateSelectionCommandHandler(repo, lookup, unit).Handle(update, token);
        updated.ClubPublicId.Should().Be(second.PublicId);
        updated.ClubSlug.Should().Be("second");
        (await db.Selections.SingleAsync(token)).ClubId.Should().Be(second.Id);
    }

    [Theory]
    [InlineData("")]
    [InlineData("bad-id")]
    [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")]
    public async Task Validators_RejectMissingMalformedOrUnknownClub(string publicId)
    {
        await using var db = NewContext();
        var lookup = new PublicIdLookup(db);
        var create = await new CreateSelectionCommandValidator(lookup).ValidateAsync(new CreateSelectionCommand { ClubPublicId = publicId });
        var update = await new UpdateSelectionCommandValidator(lookup).ValidateAsync(new UpdateSelectionCommand { ClubPublicId = publicId });
        create.Errors.Should().Contain(x => x.PropertyName == "ClubPublicId");
        update.Errors.Should().Contain(x => x.PropertyName == "ClubPublicId");
    }

    [Fact]
    public async Task ClubFilter_PagesActiveSelections_AndDeletedClubIsNotExposed()
    {
        await using var db = NewContext();
        var country = new Country { Name = "Country", Slug = "country" };
        var club = new Club { Name = "Club", Slug = "club" };
        var other = new Club { Name = "Other", Slug = "other" };
        db.AddRange(country, club, other);
        await db.SaveChangesAsync();
        var alpha = new SelectionEntity { Name = "Alpha", Slug = "alpha", CountryId = country.Id, ClubId = club.Id };
        var beta = new SelectionEntity { Name = "Beta", Slug = "beta", CountryId = country.Id, ClubId = club.Id };
        var deleted = new SelectionEntity { Name = "Deleted", Slug = "deleted", CountryId = country.Id, ClubId = club.Id };
        db.AddRange(alpha, beta, deleted, new SelectionEntity { Name = "Other", Slug = "other", CountryId = country.Id, ClubId = other.Id },
            new SelectionEntity { Name = "Legacy", Slug = "legacy", CountryId = country.Id });
        await db.SaveChangesAsync();
        db.Remove(deleted);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        var repo = new SelectionRepository(db);
        var handler = new GetAllSelectionsQueryHandler(repo);
        var page = await handler.Handle(new GetAllSelectionsQuery { ClubPublicId = club.PublicId, Page = 2, PageSize = 1 }, default);
        page.TotalCount.Should().Be(2);
        page.Items.Should().ContainSingle(x => x.Name == "Beta" && x.ClubName == "Club" && x.ClubSlug == "club");
        var combined = await handler.Handle(new GetAllSelectionsQuery { ClubPublicId = club.PublicId, Name = "Alpha", CountryPublicId = country.PublicId }, default);
        combined.Items.Should().ContainSingle(x => x.Name == "Alpha");

        db.Remove(await db.Clubs.SingleAsync(x => x.Id == club.Id));
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        var detail = await new GetSelectionBySlugQueryHandler(repo).Handle(new GetSelectionBySlugQuery { Slug = "alpha" }, default);
        detail.ClubPublicId.Should().BeNull();
        detail.ClubName.Should().BeNull();
        detail.ClubSlug.Should().BeNull();
        (await handler.Handle(new GetAllSelectionsQuery { ClubPublicId = club.PublicId }, default)).TotalCount.Should().Be(0);
        var validation = await new CreateSelectionCommandValidator(new PublicIdLookup(db)).ValidateAsync(new CreateSelectionCommand { ClubPublicId = club.PublicId });
        validation.Errors.Should().Contain(x => x.PropertyName == "ClubPublicId");
    }
}
