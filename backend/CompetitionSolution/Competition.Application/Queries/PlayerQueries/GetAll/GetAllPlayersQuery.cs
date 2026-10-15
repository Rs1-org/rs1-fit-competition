using Competition.Application.Common.Models;
using Competition.Application.DTOs.PersonDto;
using FluentValidation;
using MediatR;

namespace Competition.Application.Features.PlayerQueries.GetAll;

public class GetAllPlayersQuery : IRequest<PagedResult<PlayerDto>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? Name { get; init; }
    public Guid? PositionId { get; init; }
    public Guid? SubPositionId { get; init; }
    public int? AgeMin { get; init; }
    public int? AgeMax { get; init; }
}

public class GetAllPlayersQueryValidator : AbstractValidator<GetAllPlayersQuery>
{
    public GetAllPlayersQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.AgeMin).GreaterThanOrEqualTo(0).When(x => x.AgeMin.HasValue);
        RuleFor(x => x.AgeMax).GreaterThanOrEqualTo(0).When(x => x.AgeMax.HasValue);
        RuleFor(x => x.AgeMax)
            .GreaterThanOrEqualTo(x => x.AgeMin)
            .When(x => x.AgeMin.HasValue && x.AgeMax.HasValue)
            .WithMessage("Age max must be greater than or equal to age min.");
        RuleFor(x => x.PositionId).NotEqual(Guid.Empty).When(x => x.PositionId.HasValue);
        RuleFor(x => x.SubPositionId).NotEqual(Guid.Empty).When(x => x.SubPositionId.HasValue);
    }
}
