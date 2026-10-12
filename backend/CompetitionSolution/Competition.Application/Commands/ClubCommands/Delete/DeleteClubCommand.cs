using Competition.Application.Interfaces;
using FluentValidation;
using MediatR;
using NodaTime;

namespace Competition.Application.Commands.ClubCommands.Delete;

public class DeleteClubCommand : IRequest<string>
{
    public string Id { get; init; } = string.Empty;
}

public class DeleteClubCommandValidator : AbstractValidator<DeleteClubCommand>
{
    public DeleteClubCommandValidator() => RuleFor(x => x.Id).NotEmpty().Length(32).Matches("^[a-fA-F0-9]{32}$");
}

public class DeleteClubCommandHandler(IClubRepository clubs, IClock clock, IUnitOfWork unitOfWork) : IRequestHandler<DeleteClubCommand, string>
{
    public async Task<string> Handle(DeleteClubCommand request, CancellationToken cancellationToken)
    {
        var club = await clubs.GetByPublicIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException("Club was not found.");
        club.DeletedAt = clock.GetCurrentInstant();
        await clubs.UpdateAsync(club, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return club.PublicId;
    }
}
