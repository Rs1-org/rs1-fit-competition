using Competition.Application.DTOs.StadiumDto;
using MediatR;

namespace Competition.Application.Queries.Stadium.GetBySlug;

public class GetStadiumBySlugQuery : IRequest<StadiumDetailsDto>
{
    public string? Slug { get; set; }
}
