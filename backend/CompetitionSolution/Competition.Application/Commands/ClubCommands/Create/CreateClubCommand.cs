using Competition.Application.DTOs.ClubDto;
using Competition.Application.Helpers;
using Competition.Application.Interfaces;
using Competition.Domain.Entities;
using Competition.Domain.Entities.MatchEntities;
using MediatR;
using NodaTime.Text;

namespace Competition.Application.Commands.ClubCommands.Create;

public class CreateClubCommand : ClubInput, IRequest<ClubResponseDto>;

public class CreateClubCommandHandler(IClubRepository clubs, IPublicIdLookup lookup, IUnitOfWork unitOfWork) : IRequestHandler<CreateClubCommand, ClubResponseDto>
{
    public async Task<ClubResponseDto> Handle(CreateClubCommand request, CancellationToken cancellationToken)
    {
        var club = new Club { Id = Guid.NewGuid(), PublicId = BaseEntity.NewPublicId() };
        await ApplyAsync(club, request, lookup, cancellationToken);
        await clubs.AddAsync(club, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        var created = await clubs.GetBySlugAsync(club.Slug, cancellationToken)
            ?? throw new KeyNotFoundException("The created club could not be loaded.");
        return ClubResponseDto.FromEntity(created);
    }

    internal static async Task ApplyAsync(Club club, ClubInput request, IPublicIdLookup lookup, CancellationToken cancellationToken)
    {
        club.Name = request.Name.Trim();
        club.Slug = SlugHelper.GenerateSlug(club.Name);
        club.FoundingDate = LocalDatePattern.Iso.Parse(request.FoundingDate).Value;
        club.ClubAge = request.ClubAge;
        club.CityId = await lookup.GetRequiredIdAsync<City>(request.CityId, cancellationToken);
        club.StadiumId = await lookup.GetRequiredIdAsync<Stadium>(request.StadiumId, cancellationToken);
    }
}
