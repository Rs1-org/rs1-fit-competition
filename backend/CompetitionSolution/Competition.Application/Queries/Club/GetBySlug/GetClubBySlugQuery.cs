using Competition.Application.DTOs.ClubDto;
using Competition.Application.Interfaces;
using FluentValidation;
using MediatR;

namespace Competition.Application.Queries.Club.GetBySlug;

public class GetClubBySlugQuery : IRequest<ClubResponseDto>
{
    public string Slug { get; init; } = string.Empty;
}

public class GetClubBySlugQueryValidator : AbstractValidator<GetClubBySlugQuery>
{
    public GetClubBySlugQueryValidator() => RuleFor(x => x.Slug).NotEmpty().MaximumLength(150);
}

public class GetClubBySlugQueryHandler(IClubRepository clubs) : IRequestHandler<GetClubBySlugQuery, ClubResponseDto>
{
    public async Task<ClubResponseDto> Handle(GetClubBySlugQuery request, CancellationToken cancellationToken)
    {
        var club = await clubs.GetBySlugAsync(request.Slug, cancellationToken)
            ?? throw new KeyNotFoundException("Club was not found.");
        return ClubResponseDto.FromEntity(club);
    }
}
