using AutoMapper;
using Competition.Application.DTOs.ClubDto;
using Competition.Application.Helpers;
using Competition.Application.Interfaces;
using MediatR;
using NodaTime.Text;

namespace Competition.Application.Commands.ClubCommands.Update;

public class UpdateClubCommandHandler(IClubRepository clubRepository, IMapper mapper, IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateClubCommand, ClubResponseDto>
{
    public async Task<ClubResponseDto> Handle(UpdateClubCommand request, CancellationToken cancellationToken)
    {
        var club = await clubRepository.GetBySlugAsync(request.Slug!);
        if (club == null)
        {
            throw new KeyNotFoundException($"Couldn't find any club with slug {request.Slug}");
        }

        club.Name = request.Name!.Trim();
        club.Slug = SlugHelper.GenerateSlug(request.Name);
        club.FoundingDate = LocalDatePattern.Iso.Parse(request.FoundingDate!).Value;
        club.CityId = request.CityId!.Value;
        club.ClubAge = request.ClubAge!.Value;
        club.StadiumId = request.StadiumId!.Value;

        await clubRepository.UpdateAsync(club);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var updatedClub = await clubRepository.GetBySlugAsync(club.Slug!);
        return mapper.Map<ClubResponseDto>(updatedClub ?? club);
    }
}
