using Competition.Application.Common.Models;
using Competition.Application.Common.Validation;
using Competition.Application.DTOs.PersonDto;
using Competition.Application.Interfaces;
using Competition.Domain.Entities.FormationEntities;
using FluentValidation;
using MediatR;

namespace Competition.Application.Queries.PlayerQueries.GetAll;

public class GetAllPlayersQuery : IRequest<PagedResult<PlayerDto>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? Name { get; init; }
    public string? PositionPublicId { get; init; }
    public string? SubPositionPublicId { get; init; }
    public int? AgeMin { get; init; }
    public int? AgeMax { get; init; }
}

public class GetAllPlayersQueryValidator : AbstractValidator<GetAllPlayersQuery>
{
    public GetAllPlayersQueryValidator(IPublicIdLookup lookup)
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.AgeMin).GreaterThanOrEqualTo(0).When(x => x.AgeMin.HasValue);
        RuleFor(x => x.AgeMax).GreaterThanOrEqualTo(0).When(x => x.AgeMax.HasValue);
        RuleFor(x => x.AgeMax)
            .GreaterThanOrEqualTo(x => x.AgeMin)
            .When(x => x.AgeMin.HasValue && x.AgeMax.HasValue)
            .WithMessage("Age max must be greater than or equal to age min.");
        RuleFor(x => x.PositionPublicId!).MustReferenceExisting<GetAllPlayersQuery, Position>(lookup)
            .When(x => !string.IsNullOrWhiteSpace(x.PositionPublicId));
        RuleFor(x => x.SubPositionPublicId!).MustReferenceExisting<GetAllPlayersQuery, SubPosition>(lookup)
            .When(x => !string.IsNullOrWhiteSpace(x.SubPositionPublicId));
    }
}
