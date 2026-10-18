using Competition.Application.Common.Validation;
using FluentValidation;
using NodaTime.Text;

namespace Competition.Application.Queries.Clubs;

public class GetAllClubsQueryValidator : AbstractValidator<GetAllClubsQuery>
{
    public GetAllClubsQueryValidator()
    {
        RuleFor(x => x.Page).InclusiveBetween(1, 1_000_000);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.Name).MaximumLength(100);
        RuleFor(x => x.CountryPublicId).MustBeValidPublicId().When(x => x.CountryPublicId is not null);
        RuleFor(x => x.CityPublicId).MustBeValidPublicId().When(x => x.CityPublicId is not null);
        RuleFor(x => x.ClubAge).IsInEnum().When(x => x.ClubAge.HasValue);
        RuleFor(x => x.SortBy).Must(x => x is "name" or "foundingDate" or "clubAge")
            .WithMessage("Sort by name, foundingDate or clubAge.");
        RuleFor(x => x.FoundedFrom).Must(x => x is null || (LocalDatePattern.Iso.Parse(x).Success && LocalDatePattern.Iso.Parse(x).Value.Year >= 1))
            .WithMessage("Founded from must be a valid date (yyyy-MM-dd).");
        RuleFor(x => x.FoundedTo).Must(x => x is null || (LocalDatePattern.Iso.Parse(x).Success && LocalDatePattern.Iso.Parse(x).Value.Year >= 1))
            .WithMessage("Founded to must be a valid date (yyyy-MM-dd).");
        RuleFor(x => x.FoundedTo).Must((q, end) => q.FoundedFrom is null || end is null
                || !LocalDatePattern.Iso.Parse(q.FoundedFrom).Success || !LocalDatePattern.Iso.Parse(end).Success
                || LocalDatePattern.Iso.Parse(q.FoundedFrom).Value <= LocalDatePattern.Iso.Parse(end).Value)
            .WithMessage("Founded to must be on or after founded from.");
    }
}

public class GetClubBySlugQueryValidator : AbstractValidator<GetClubBySlugQuery>
{
    public GetClubBySlugQueryValidator() => RuleFor(x => x.Slug).NotEmpty().MaximumLength(120);
}
