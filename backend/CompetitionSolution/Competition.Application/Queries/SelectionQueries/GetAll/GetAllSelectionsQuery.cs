using Competition.Application.Commands.SelectionCommands.Create;
using Competition.Application.Common.Models;
using Competition.Application.Common.Validation;
using Competition.Application.DTOs.SelectionDto;
using Competition.Application.Interfaces;
using FluentValidation;
using MediatR;

namespace Competition.Application.Queries.SelectionQueries.GetAll;

public class GetAllSelectionsQuery : IRequest<PagedResult<SelectionDto>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? Name { get; init; }
    public string? CountryPublicId { get; init; }
    public string? Abbreviation { get; init; }
    public int? FollowersMin { get; init; }
    public int? FollowersMax { get; init; }
}

public class GetAllSelectionsQueryValidator : AbstractValidator<GetAllSelectionsQuery>
{
    public GetAllSelectionsQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.Name).MaximumLength(120);
        RuleFor(x => x.Abbreviation).MaximumLength(10);
        RuleFor(x => x.CountryPublicId)
            .MustBeValidPublicId()
            .When(x => !string.IsNullOrWhiteSpace(x.CountryPublicId));
        RuleFor(x => x.FollowersMin).GreaterThanOrEqualTo(0).When(x => x.FollowersMin.HasValue);
        RuleFor(x => x.FollowersMax).GreaterThanOrEqualTo(0).When(x => x.FollowersMax.HasValue);
        RuleFor(x => x.FollowersMax)
            .GreaterThanOrEqualTo(x => x.FollowersMin)
            .When(x => x.FollowersMin.HasValue && x.FollowersMax.HasValue)
            .WithMessage("Followers max must be greater than or equal to followers min.");
    }
}

public class GetAllSelectionsQueryHandler : IRequestHandler<GetAllSelectionsQuery, PagedResult<SelectionDto>>
{
    private readonly ISelection _selections;

    public GetAllSelectionsQueryHandler(ISelection selections) => _selections = selections;

    public async Task<PagedResult<SelectionDto>> Handle(GetAllSelectionsQuery request, CancellationToken cancellationToken)
    {
        var filter = new SelectionListFilter(
            request.Name,
            request.CountryPublicId,
            request.Abbreviation,
            request.FollowersMin,
            request.FollowersMax);
        var paged = await _selections.GetPagedAsync(filter, request.Page, request.PageSize, cancellationToken);
        var items = paged.Items.Select(CreateSelectionCommandHandler.Map).ToList();
        return new PagedResult<SelectionDto>(items, paged.TotalCount, paged.Page, paged.PageSize);
    }
}
