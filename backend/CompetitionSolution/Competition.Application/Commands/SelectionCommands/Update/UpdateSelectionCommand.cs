using Competition.Application.Commands.SelectionCommands.Create;
using Competition.Application.DTOs.SelectionDto;
using Competition.Application.Helpers;
using Competition.Application.Interfaces;
using FluentValidation;
using MediatR;
using System.Text.Json.Serialization;

namespace Competition.Application.Commands.SelectionCommands.Update;

public class UpdateSelectionCommand : IRequest<SelectionDto>
{
    [JsonIgnore]
    public string Slug { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Abbreviation { get; set; } = string.Empty;
    public string? PictureUrl { get; set; }
    public int FollowersCount { get; set; }
    public Guid CountryId { get; set; }
}

public class UpdateSelectionCommandValidator : AbstractValidator<UpdateSelectionCommand>
{
    public UpdateSelectionCommandValidator()
    {
        RuleFor(x => x.Slug).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Abbreviation).NotEmpty().MaximumLength(10);
        RuleFor(x => x.CountryId).NotEmpty();
        RuleFor(x => x.FollowersCount).GreaterThanOrEqualTo(0);
    }
}

public class UpdateSelectionCommandHandler : IRequestHandler<UpdateSelectionCommand, SelectionDto>
{
    private readonly ISelection _selections;
    private readonly ICountry _countries;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateSelectionCommandHandler(ISelection selections, ICountry countries, IUnitOfWork unitOfWork)
    {
        _selections = selections;
        _countries = countries;
        _unitOfWork = unitOfWork;
    }

    public async Task<SelectionDto> Handle(UpdateSelectionCommand request, CancellationToken cancellationToken)
    {
        var entity = await _selections.GetBySlugAsync(request.Slug, cancellationToken)
            ?? throw new KeyNotFoundException($"Selection '{request.Slug}' was not found.");

        var country = await _countries.GetByIdAsync(request.CountryId, cancellationToken)
            ?? throw new KeyNotFoundException($"Country '{request.CountryId}' was not found.");

        var slug = SlugHelper.GenerateSlug(request.Name);
        var existing = await _selections.GetBySlugAsync(slug, cancellationToken);
        if (existing != null && existing.Id != entity.Id)
            throw new InvalidOperationException($"Selection '{request.Name}' already exists.");

        entity.Name = request.Name.Trim();
        entity.Abbreviation = request.Abbreviation.Trim().ToUpperInvariant();
        if (!string.IsNullOrWhiteSpace(request.PictureUrl))
            entity.PictureUrl = request.PictureUrl.Trim();
        entity.FollowersCount = request.FollowersCount;
        entity.CountryId = country.Id;
        entity.Slug = slug;

        await _selections.UpdateAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var updated = await _selections.GetBySlugAsync(slug, cancellationToken);
        return CreateSelectionCommandHandler.Map(updated!);
    }
}
