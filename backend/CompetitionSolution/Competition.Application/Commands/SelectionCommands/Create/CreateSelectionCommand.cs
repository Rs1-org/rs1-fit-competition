using Competition.Application.Common.Validation;
using Competition.Application.DTOs.SelectionDto;
using Competition.Application.Helpers;
using Competition.Application.Interfaces;
using Competition.Domain.Entities;
using Competition.Domain.Entities.MatchEntities;
using FluentValidation;
using MediatR;

namespace Competition.Application.Commands.SelectionCommands.Create;

public class CreateSelectionCommand : IRequest<SelectionDto>
{
    public string Name { get; set; } = string.Empty;
    public string Abbreviation { get; set; } = string.Empty;
    public string? PictureUrl { get; set; }
    public int FollowersCount { get; set; }
    public string ClubPublicId { get; set; } = string.Empty;
    public string CountryPublicId { get; set; } = string.Empty;
    public string? HomeStadiumPublicId { get; set; }
}

public class CreateSelectionCommandValidator : AbstractValidator<CreateSelectionCommand>
{
    public CreateSelectionCommandValidator(IPublicIdLookup lookup)
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Abbreviation).NotEmpty().MaximumLength(10);
        RuleFor(x => x.CountryPublicId).MustReferenceExisting<CreateSelectionCommand, Country>(lookup);
        RuleFor(x => x.ClubPublicId).MustReferenceExisting<CreateSelectionCommand, Club>(lookup);
        RuleFor(x => x.HomeStadiumPublicId).MustReferenceExisting<CreateSelectionCommand, Stadium>(lookup)
            .When(x => !string.IsNullOrWhiteSpace(x.HomeStadiumPublicId));
        RuleFor(x => x.FollowersCount).GreaterThanOrEqualTo(0);
    }
}

public class CreateSelectionCommandHandler : IRequestHandler<CreateSelectionCommand, SelectionDto>
{
    private readonly ISelection _selections;
    private readonly IPublicIdLookup _publicIds;
    private readonly IUnitOfWork _unitOfWork;

    public CreateSelectionCommandHandler(ISelection selections, IPublicIdLookup publicIds, IUnitOfWork unitOfWork)
    {
        _selections = selections;
        _publicIds = publicIds;
        _unitOfWork = unitOfWork;
    }

    public async Task<SelectionDto> Handle(CreateSelectionCommand request, CancellationToken cancellationToken)
    {
        var clubId = await _publicIds.GetRequiredIdAsync<Club>(request.ClubPublicId, cancellationToken);
        var countryId = await _publicIds.GetRequiredIdAsync<Country>(request.CountryPublicId, cancellationToken);
        Guid? homeStadiumId = string.IsNullOrWhiteSpace(request.HomeStadiumPublicId)
            ? null
            : await _publicIds.GetRequiredIdAsync<Stadium>(request.HomeStadiumPublicId, cancellationToken);

        var slug = SlugHelper.GenerateSlug(request.Name);
        if (await _selections.GetBySlugAsync(slug, cancellationToken) != null)
            throw new InvalidOperationException($"Selection '{request.Name}' already exists.");

        var entity = new Selection
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Abbreviation = request.Abbreviation.Trim().ToUpperInvariant(),
            PictureUrl = string.IsNullOrWhiteSpace(request.PictureUrl) ? null : request.PictureUrl.Trim(),
            FollowersCount = request.FollowersCount,
            ClubId = clubId,
            CountryId = countryId,
            HomeStadiumId = homeStadiumId,
            Slug = slug
        };

        await _selections.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var created = await _selections.GetBySlugAsync(slug, cancellationToken);
        return Map(created!);
    }

    internal static SelectionDto Map(Selection entity) => new()
    {
        Id = entity.Id,
        Name = entity.Name,
        Abbreviation = entity.Abbreviation,
        PictureUrl = entity.PictureUrl,
        FollowersCount = entity.FollowersCount,
        Slug = entity.Slug,
        ClubPublicId = entity.Club?.DeletedAt == null ? entity.Club?.PublicId : null,
        ClubName = entity.Club?.DeletedAt == null ? entity.Club?.Name : null,
        ClubSlug = entity.Club?.DeletedAt == null ? entity.Club?.Slug : null,
        CountryPublicId = entity.Country?.PublicId,
        CountryName = entity.Country?.Name,
        HomeStadiumPublicId = entity.HomeStadium?.DeletedAt == null ? entity.HomeStadium?.PublicId : null,
        StadiumName = entity.HomeStadium?.DeletedAt == null ? entity.HomeStadium?.Name : null,
        StadiumSlug = entity.HomeStadium?.DeletedAt == null ? entity.HomeStadium?.Slug : null
    };
}
