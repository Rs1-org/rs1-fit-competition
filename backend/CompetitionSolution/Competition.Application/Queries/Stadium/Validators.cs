using Competition.Application.Queries.Stadium.GetAll;
using Competition.Application.Queries.Stadium.GetBySlug;
using Competition.Application.Common.Validation;
using Competition.Application.Common.Models;
using Competition.Application.Common.ImportExport;
using Competition.Application.Queries.Stadium.Export;
using FluentValidation;

namespace Competition.Application.Queries.Stadium;

public class GetStadiumBySlugQueryValidator : AbstractValidator<GetStadiumBySlugQuery>
{
    public GetStadiumBySlugQueryValidator()
    {
        RuleFor(s => s.Slug).NotEmpty();
    }
}

public class GetAllStadiumsQueryValidator : AbstractValidator<GetAllStadiumsQuery>
{
    public GetAllStadiumsQueryValidator()
    {
        RuleFor(s => s.Page).GreaterThanOrEqualTo(1);
        RuleFor(s => s.PageSize).InclusiveBetween(1, 100);
        RuleFor(s => s.Name).MaximumLength(150);
        RuleFor(s => s.SortBy).MustBeSortKey(SortKeys.Stadium);
        RuleFor(s => s.SortDir).MustBeSortDirection();
        RuleFor(s => s.CountryPublicId).MustBeValidPublicId().When(s => s.CountryPublicId is not null);
        RuleFor(s => s.CityPublicId).MustBeValidPublicId().When(s => s.CityPublicId is not null);
        RuleFor(s => s.MinCapacity).GreaterThanOrEqualTo(0).When(s => s.MinCapacity.HasValue);
        RuleFor(s => s.MaxCapacity).GreaterThanOrEqualTo(0).When(s => s.MaxCapacity.HasValue);
        RuleFor(s => s.MaxCapacity)
            .GreaterThanOrEqualTo(s => s.MinCapacity)
            .When(s => s.MinCapacity.HasValue && s.MaxCapacity.HasValue)
            .WithMessage("'Max capacity' must be greater than or equal to 'Min capacity'.");
    }
}

public class ExportStadiumsQueryValidator : AbstractValidator<ExportStadiumsQuery>
{
    public ExportStadiumsQueryValidator()
    {
        RuleFor(s => s.Format).MustBeExportFormat();
        RuleFor(s => s.Name).MaximumLength(150);
        RuleFor(s => s.SortBy).MustBeSortKey(SortKeys.Stadium);
        RuleFor(s => s.SortDir).MustBeSortDirection();
        RuleFor(s => s.CountryPublicId).MustBeValidPublicId().When(s => s.CountryPublicId is not null);
        RuleFor(s => s.CityPublicId).MustBeValidPublicId().When(s => s.CityPublicId is not null);
        RuleFor(s => s.MinCapacity).GreaterThanOrEqualTo(0).When(s => s.MinCapacity.HasValue);
        RuleFor(s => s.MaxCapacity)
            .GreaterThanOrEqualTo(s => s.MinCapacity)
            .When(s => s.MinCapacity.HasValue && s.MaxCapacity.HasValue)
            .WithMessage("'Max capacity' must be greater than or equal to 'Min capacity'.");
    }
}
