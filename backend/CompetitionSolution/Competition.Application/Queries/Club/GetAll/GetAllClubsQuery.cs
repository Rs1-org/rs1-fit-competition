using Competition.Application.Common.Models;
using Competition.Application.DTOs.ClubDto;
using Competition.Application.Interfaces;
using FluentValidation;
using MediatR;

namespace Competition.Application.Queries.Club.GetAll;

public class GetAllClubsQuery : ClubListFilter, IRequest<PagedResult<ClubResponseDto>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

public class GetAllClubsQueryValidator : AbstractValidator<GetAllClubsQuery>
{
    public GetAllClubsQueryValidator()
    {
        RuleFor(x => x.Page).InclusiveBetween(1, 1000000);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.Name).MaximumLength(100);
        RuleFor(x => x.CityId).Length(32).Matches("^[a-fA-F0-9]{32}$").When(x => x.CityId != null);
        RuleFor(x => x.StadiumId).Length(32).Matches("^[a-fA-F0-9]{32}$").When(x => x.StadiumId != null);
        RuleFor(x => x.FoundingYear).InclusiveBetween(1, 9999).When(x => x.FoundingYear != null);
        RuleFor(x => x.ClubAge).IsInEnum().When(x => x.ClubAge != null);
    }
}

public class GetAllClubsQueryHandler(IClubRepository clubs) : IRequestHandler<GetAllClubsQuery, PagedResult<ClubResponseDto>>
{
    public async Task<PagedResult<ClubResponseDto>> Handle(GetAllClubsQuery request, CancellationToken cancellationToken)
    {
        var page = await clubs.GetPagedFilteredAsync(request, request.Page, request.PageSize, cancellationToken);
        return new PagedResult<ClubResponseDto>(page.Items.Select(ClubResponseDto.FromEntity).ToList(), page.TotalCount, page.Page, page.PageSize);
    }
}
