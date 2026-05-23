using AutoMapper;
using Competition.Application.DTOs.ClubDto;
using Competition.Application.Interfaces;
using MediatR;

namespace Competition.Application.Queries.Club.GetAll;

public class GetAllClubsQueryHandler(IClubRepository clubRepository, IMapper mapper)
    : IRequestHandler<GetAllClubsQuery, IEnumerable<ClubResponseDto>>
{
    public async Task<IEnumerable<ClubResponseDto>> Handle(GetAllClubsQuery request, CancellationToken cancellationToken)
    {
        var clubs = await clubRepository.GetAllAsync();
        return mapper.Map<IEnumerable<ClubResponseDto>>(clubs);
    }
}
