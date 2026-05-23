using AutoMapper;
using Competition.Application.DTOs.ClubDto;
using Competition.Application.Interfaces;
using MediatR;

namespace Competition.Application.Queries.Club.GetBySlug;

public class GetClubBySlugQueryHandler(IClubRepository clubRepository, IMapper mapper)
    : IRequestHandler<GetClubBySlugQuery, ClubResponseDto>
{
    public async Task<ClubResponseDto> Handle(GetClubBySlugQuery request, CancellationToken cancellationToken)
    {
        var club = await clubRepository.GetBySlugAsync(request.Slug!);
        if (club == null)
        {
            throw new KeyNotFoundException($"Couldn't find any club with slug {request.Slug}");
        }

        return mapper.Map<ClubResponseDto>(club);
    }
}
