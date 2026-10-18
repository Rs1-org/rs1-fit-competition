using System.Text.Json.Serialization;
using AutoMapper;
using Competition.Application.DTOs.ClubDto;
using Competition.Application.Helpers;
using Competition.Application.Interfaces;
using Competition.Domain.Entities;
using Competition.Domain.Entities.MatchEntities;
using MediatR;

namespace Competition.Application.Commands.ClubCommands;

public abstract class SaveClubCommand
{
    public string Name { get; set; } = string.Empty;
    public string? FoundingDate { get; set; }
    public ClubAge ClubAge { get; set; } = ClubAge.Senior;
    public string? CityPublicId { get; set; }
    public string? StadiumPublicId { get; set; }
}

public class CreateClubCommand : SaveClubCommand, IRequest<ClubDetailsDto>;

public class UpdateClubCommand : SaveClubCommand, IRequest<ClubDetailsDto>
{
    [JsonIgnore] public string Slug { get; set; } = string.Empty;
}

public record DeleteClubCommand(string PublicId) : IRequest<DeletedClubDto>;

internal static class ClubCommandValues
{
    public static async Task ApplyAsync(Club club, SaveClubCommand request, IClub repository,
        IPublicIdLookup lookup, CancellationToken ct)
    {
        var name = request.Name.Trim();
        var slug = SlugHelper.GenerateSlug(name);
        if (await repository.SlugExistsAsync(slug, club.Id, ct))
            throw BusinessRule.Violation(nameof(request.Name), "A club with this name or a similar name already exists.");

        var cityId = await ResolveAsync<City>(request.CityPublicId, nameof(request.CityPublicId), lookup, ct);
        var stadiumId = await ResolveAsync<Stadium>(request.StadiumPublicId, nameof(request.StadiumPublicId), lookup, ct);
        club.Name = name;
        club.Slug = slug;
        club.FoundingDate = NodaTimeMappingHelper.ParseNullableLocalDate(request.FoundingDate);
        club.ClubAge = request.ClubAge;
        club.CityId = cityId;
        club.StadiumId = stadiumId;
        club.City = null;
        club.Stadium = null;
    }

    private static async Task<Guid?> ResolveAsync<T>(string? publicId, string property,
        IPublicIdLookup lookup, CancellationToken ct) where T : BaseEntity
    {
        if (publicId is null) return null;
        return await lookup.GetIdByPublicIdAsync<T>(publicId, ct)
            ?? throw BusinessRule.Violation(property, "Select an existing, active record.");
    }
}

public class CreateClubCommandHandler(IClub clubs, IPublicIdLookup lookup, IUnitOfWork unitOfWork, IMapper mapper)
    : IRequestHandler<CreateClubCommand, ClubDetailsDto>
{
    public async Task<ClubDetailsDto> Handle(CreateClubCommand request, CancellationToken cancellationToken)
    {
        var club = new Club();
        await ClubCommandValues.ApplyAsync(club, request, clubs, lookup, cancellationToken);
        await clubs.AddAsync(club, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return mapper.Map<ClubDetailsDto>(await clubs.GetBySlugAsync(club.Slug, cancellationToken));
    }
}

public class UpdateClubCommandHandler(IClub clubs, IPublicIdLookup lookup, IUnitOfWork unitOfWork, IMapper mapper)
    : IRequestHandler<UpdateClubCommand, ClubDetailsDto>
{
    public async Task<ClubDetailsDto> Handle(UpdateClubCommand request, CancellationToken cancellationToken)
    {
        var club = await clubs.GetBySlugAsync(request.Slug, cancellationToken)
            ?? throw new KeyNotFoundException("Club not found.");
        await ClubCommandValues.ApplyAsync(club, request, clubs, lookup, cancellationToken);
        await clubs.UpdateAsync(club, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return mapper.Map<ClubDetailsDto>(await clubs.GetBySlugAsync(club.Slug, cancellationToken));
    }
}

public class DeleteClubCommandHandler(IClub clubs, IPublicIdLookup lookup, IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteClubCommand, DeletedClubDto>
{
    public async Task<DeletedClubDto> Handle(DeleteClubCommand request, CancellationToken cancellationToken)
    {
        var club = await lookup.GetRequiredEntityAsync<Club>(request.PublicId, cancellationToken);
        await clubs.DeleteAsync(club, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new DeletedClubDto(request.PublicId);
    }
}
