using Competition.Application.Commands.ClubCommands.Create;
using Competition.Application.Commands.ClubCommands.Update;
using Competition.Application.Helpers;
using Competition.Application.Interfaces;
using Competition.Domain.Entities;
using Competition.Domain.Entities.MatchEntities;
using FluentValidation;
using NodaTime;
using NodaTime.Text;

namespace Competition.Application.Commands.ClubCommands.Validators;

public class ClubInputValidator<T> : AbstractValidator<T> where T : ClubInput
{
    public ClubInputValidator(IPublicIdLookup lookup, IStadium stadiums, IClock clock)
    {
        RuleFor(x => x.Name).Cascade(CascadeMode.Stop).NotEmpty().MaximumLength(100)
            .Must(x => !string.IsNullOrWhiteSpace(SlugHelper.GenerateSlug(x))).WithMessage("Name must contain letters or numbers.");
        RuleFor(x => x.FoundingDate).Cascade(CascadeMode.Stop).NotEmpty()
            .Must(x => LocalDatePattern.Iso.Parse(x).Success).WithMessage("Founding date must use yyyy-MM-dd.")
            .Must(x => LocalDatePattern.Iso.Parse(x).Value <= clock.GetCurrentInstant().InUtc().Date).WithMessage("Founding date cannot be in the future.");
        RuleFor(x => x.ClubAge).IsInEnum();
        RuleFor(x => x.CityId).Cascade(CascadeMode.Stop).NotEmpty().Length(32).Matches("^[a-fA-F0-9]{32}$")
            .MustAsync(async (id, ct) => await lookup.GetIdByPublicIdAsync<City>(id, ct) != null).WithMessage("City was not found.");
        RuleFor(x => x.StadiumId).Cascade(CascadeMode.Stop).NotEmpty().Length(32).Matches("^[a-fA-F0-9]{32}$")
            .MustAsync(async (id, ct) => await lookup.GetIdByPublicIdAsync<Stadium>(id, ct) != null).WithMessage("Stadium was not found.");
        RuleFor(x => x).MustAsync(async (input, ct) =>
        {
            var cityId = await lookup.GetIdByPublicIdAsync<City>(input.CityId, ct);
            var stadiumId = await lookup.GetIdByPublicIdAsync<Stadium>(input.StadiumId, ct);
            if (cityId == null || stadiumId == null) return true;
            var stadium = await stadiums.GetByIdAsync(stadiumId.Value, ct);
            return stadium != null && stadium.CityId == cityId;
        }).WithMessage("Stadium must belong to the selected city.");
    }
}

public class CreateClubCommandValidator : AbstractValidator<CreateClubCommand>
{
    public CreateClubCommandValidator(IPublicIdLookup lookup, IStadium stadiums, IClubRepository clubs, IClock clock)
    {
        Include(new ClubInputValidator<CreateClubCommand>(lookup, stadiums, clock));
        RuleFor(x => x.Name).MustAsync(async (name, ct) =>
            string.IsNullOrWhiteSpace(name) || await clubs.GetBySlugAsync(SlugHelper.GenerateSlug(name), ct) == null)
            .WithMessage("A club with this name already exists.");
    }
}

public class UpdateClubCommandValidator : AbstractValidator<UpdateClubCommand>
{
    public UpdateClubCommandValidator(IPublicIdLookup lookup, IStadium stadiums, IClubRepository clubs, IClock clock)
    {
        Include(new ClubInputValidator<UpdateClubCommand>(lookup, stadiums, clock));
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Name).MustAsync(async (input, name, ct) =>
        {
            if (string.IsNullOrWhiteSpace(name)) return true;
            var existing = await clubs.GetBySlugAsync(SlugHelper.GenerateSlug(name), ct);
            return existing == null || existing.Slug == input.Slug;
        }).WithMessage("A club with this name already exists.");
    }
}
