using Competition.Application.DTOs.ClubDto;
using MediatR;

namespace Competition.Application.Queries.Club.GetBySlug;

public class GetClubBySlugQuery : IRequest<ClubResponseDto>
{
    public string? Slug { get; set; }
}
