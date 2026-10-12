using System.Text.Json.Serialization;
using Competition.Application.Commands.ClubCommands.Create;
using Competition.Application.DTOs.ClubDto;
using Competition.Application.Interfaces;
using MediatR;

namespace Competition.Application.Commands.ClubCommands.Update;

public class UpdateClubCommand : ClubInput, IRequest<ClubResponseDto>
{
    [JsonIgnore] public string Slug { get; set; } = string.Empty;
}

public class UpdateClubCommandHandler(IClubRepository clubs, IPublicIdLookup lookup, IUnitOfWork unitOfWork) : IRequestHandler<UpdateClubCommand, ClubResponseDto>
{
    public async Task<ClubResponseDto> Handle(UpdateClubCommand request, CancellationToken cancellationToken)
    {
        var club = await clubs.GetBySlugAsync(request.Slug, cancellationToken)
            ?? throw new KeyNotFoundException("Club was not found.");
        await CreateClubCommandHandler.ApplyAsync(club, request, lookup, cancellationToken);
        await clubs.UpdateAsync(club, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        var updated = await clubs.GetBySlugAsync(club.Slug, cancellationToken)
            ?? throw new KeyNotFoundException("The updated club could not be loaded.");
        return ClubResponseDto.FromEntity(updated);
    }
}
