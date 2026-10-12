using Competition.Application.Queries.Stadium.GetAll;
using Competition.Application.Queries.Stadium.GetBySlug;
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
        RuleFor(s => s.CountryId).NotEqual(Guid.Empty).When(s => s.CountryId.HasValue);
        RuleFor(s => s.CityId).NotEqual(Guid.Empty).When(s => s.CityId.HasValue);
        RuleFor(s => s.MinCapacity).GreaterThanOrEqualTo(0).When(s => s.MinCapacity.HasValue);
        RuleFor(s => s.MaxCapacity).GreaterThanOrEqualTo(0).When(s => s.MaxCapacity.HasValue);
        RuleFor(s => s.MaxCapacity)
            .GreaterThanOrEqualTo(s => s.MinCapacity)
            .When(s => s.MinCapacity.HasValue && s.MaxCapacity.HasValue)
            .WithMessage("'Max capacity' must be greater than or equal to 'Min capacity'.");
    }
}
