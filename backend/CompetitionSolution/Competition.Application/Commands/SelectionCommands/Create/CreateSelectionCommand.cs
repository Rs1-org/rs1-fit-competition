using Competition.Application.DTOs.SelectionDto;
using Competition.Application.Helpers;
using Competition.Application.Interfaces;
using Competition.Domain.Entities;
using FluentValidation;
using MediatR;

namespace Competition.Application.Commands.SelectionCommands.Create;

public class CreateSelectionCommand : IRequest<SelectionDto>
{
    public string Name { get; set; } = string.Empty;
    public string Abbreviation { get; set; } = string.Empty;
    public string? PictureUrl { get; set; }
    public int FollowersCount { get; set; }
    public Guid CountryId { get; set; }
}

public class CreateSelectionCommandValidator : AbstractValidator<CreateSelectionCommand>
{
    public CreateSelectionCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Abbreviation).NotEmpty().MaximumLength(10);
        RuleFor(x => x.CountryId).NotEmpty();
        RuleFor(x => x.FollowersCount).GreaterThanOrEqualTo(0);
    }
}

public class CreateSelectionCommandHandler : IRequestHandler<CreateSelectionCommand, SelectionDto>
{
    private readonly ISelection _selections;
    private readonly ICountry _countries;
    private readonly IUnitOfWork _unitOfWork;

    public CreateSelectionCommandHandler(ISelection selections, ICountry countries, IUnitOfWork unitOfWork)
    {
        _selections = selections;
        _countries = countries;
        _unitOfWork = unitOfWork;
    }

    public async Task<SelectionDto> Handle(CreateSelectionCommand request, CancellationToken cancellationToken)
    {
        var country = await _countries.GetByIdAsync(request.CountryId, cancellationToken)
            ?? throw new KeyNotFoundException($"Country '{request.CountryId}' was not found.");

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
            CountryId = country.Id,
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
        CountryId = entity.CountryId,
        CountryName = entity.Country?.Name
    };
}
