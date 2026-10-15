using Competition.Application.Common.Models;
using Competition.Application.DTOs.PersonDto;
using FluentValidation;
using MediatR;

namespace Competition.Application.Queries.RefereeQueries.GetAll;

public class GetAllRefereesQuery : IRequest<PagedResult<RefereeDto>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? Name { get; init; }
    public int? AgeMin { get; init; }
    public int? AgeMax { get; init; }
    public DateOnly? CareerStartFrom { get; init; }
    public DateOnly? CareerStartTo { get; init; }
}

public class GetAllRefereesQueryValidator : AbstractValidator<GetAllRefereesQuery>
{
    public GetAllRefereesQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.AgeMin).GreaterThanOrEqualTo(0).When(x => x.AgeMin.HasValue);
        RuleFor(x => x.AgeMax).GreaterThanOrEqualTo(0).When(x => x.AgeMax.HasValue);
        RuleFor(x => x.AgeMax)
            .GreaterThanOrEqualTo(x => x.AgeMin)
            .When(x => x.AgeMin.HasValue && x.AgeMax.HasValue)
            .WithMessage("Age max must be greater than or equal to age min.");
        RuleFor(x => x.CareerStartTo)
            .GreaterThanOrEqualTo(x => x.CareerStartFrom)
            .When(x => x.CareerStartFrom.HasValue && x.CareerStartTo.HasValue);
    }
}
