using AutoMapper;
using Competition.Application.DTOs.ClubDto;
using Competition.Application.Helpers;
using Competition.Application.Interfaces;
using Competition.Domain.Entities;
using MediatR;
using NodaTime.Text;

namespace Competition.Application.Commands.ClubCommands.Create;

public class CreateClubCommandHandler(IClubRepository clubRepository, IMapper mapper, IUnitOfWork unitOfWork)
    : IRequestHandler<CreateClubCommand, ClubResponseDto>
{
    public async Task<ClubResponseDto> Handle(CreateClubCommand request, CancellationToken cancellationToken)
    {
        var club = new Club
        {
            Name = request.Name!.Trim(),
            Slug = SlugHelper.GenerateSlug(request.Name ?? string.Empty),
            FoundingDate = LocalDatePattern.Iso.Parse(request.FoundingDate!).Value,
            CityId = request.CityId!.Value,
            ClubAge = request.ClubAge!.Value,
            StadiumId = request.StadiumId!.Value
        };

        await clubRepository.AddAsync(club);
        var result = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (result <= 0)
        {
            throw new InvalidOperationException("Failed to save club");
        }

        var createdClub = await clubRepository.GetDetailedByIdAsync(club.Id);
        return mapper.Map<ClubResponseDto>(createdClub ?? club);
    }
}
