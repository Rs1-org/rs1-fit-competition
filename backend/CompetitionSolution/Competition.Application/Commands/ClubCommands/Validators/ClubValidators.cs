using Competition.Application.Commands.ClubCommands.Create;
using Competition.Application.Commands.ClubCommands.Delete;
using Competition.Application.Commands.ClubCommands.Update;
using Competition.Application.Helpers;
using Competition.Application.Interfaces;
using FluentValidation;
using NodaTime.Text;

namespace Competition.Application.Commands.ClubCommands.Validators;

public class CreateClubCommandValidator : AbstractValidator<CreateClubCommand>
{
    public CreateClubCommandValidator(ICity cityRepository, IStadium stadiumRepository, IClubRepository clubRepository)
    {
        RuleFor(x => x.Name)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .MaximumLength(100)
            .Must(name => name == null || name.Trim().Length > 0)
            .WithMessage("Name must not be empty.")
            .MustAsync(async (name, ct) =>
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    return true;
                }

                return await clubRepository.GetBySlugAsync(SlugHelper.GenerateSlug(name)) == null;
            })
            .WithMessage("A club with this name already exists.");

        AddSharedRules(cityRepository, stadiumRepository);
    }

    private void AddSharedRules(ICity cityRepository, IStadium stadiumRepository)
    {
        RuleFor(x => x.FoundingDate)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .Must(BeValidIsoDate)
            .WithMessage("FoundingDate must be in yyyy-MM-dd format.")
            .Must(NotBeInFuture)
            .WithMessage("FoundingDate cannot be in the future.");

        RuleFor(x => x.ClubAge)
            .NotNull()
            .IsInEnum();

        RuleFor(x => x.CityId)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .MustAsync(async (cityId, ct) => cityId.HasValue && await cityRepository.GetByIdAsync(cityId.Value) != null)
            .WithMessage("The city does not exist.");

        RuleFor(x => x.StadiumId)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .MustAsync(async (stadiumId, ct) => stadiumId.HasValue && await stadiumRepository.GetByIdAsync(stadiumId.Value) != null)
            .WithMessage("The stadium does not exist.");
    }

    private static bool BeValidIsoDate(string? value)
    {
        return !string.IsNullOrWhiteSpace(value) && LocalDatePattern.Iso.Parse(value).Success;
    }

    private static bool NotBeInFuture(string? value)
    {
        var parseResult = LocalDatePattern.Iso.Parse(value ?? string.Empty);
        if (!parseResult.Success)
        {
            return true;
        }

        var today = NodaTime.LocalDate.FromDateTime(DateTime.UtcNow.Date);
        return parseResult.Value <= today;
    }
}

public class UpdateClubCommandValidator : AbstractValidator<UpdateClubCommand>
{
    public UpdateClubCommandValidator(ICity cityRepository, IStadium stadiumRepository, IClubRepository clubRepository)
    {
        RuleFor(x => x.Slug).NotEmpty();

        RuleFor(x => x.Name)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .MaximumLength(100)
            .Must(name => name == null || name.Trim().Length > 0)
            .WithMessage("Name must not be empty.")
            .MustAsync(async (command, name, ct) =>
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    return true;
                }

                var existingClub = await clubRepository.GetBySlugAsync(SlugHelper.GenerateSlug(name));
                return existingClub == null || existingClub.Slug == command.Slug;
            })
            .WithMessage("A club with this name already exists.");

        RuleFor(x => x.FoundingDate)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .Must(BeValidIsoDate)
            .WithMessage("FoundingDate must be in yyyy-MM-dd format.")
            .Must(NotBeInFuture)
            .WithMessage("FoundingDate cannot be in the future.");

        RuleFor(x => x.ClubAge)
            .NotNull()
            .IsInEnum();

        RuleFor(x => x.CityId)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .MustAsync(async (cityId, ct) => cityId.HasValue && await cityRepository.GetByIdAsync(cityId.Value) != null)
            .WithMessage("The city does not exist.");

        RuleFor(x => x.StadiumId)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .MustAsync(async (stadiumId, ct) => stadiumId.HasValue && await stadiumRepository.GetByIdAsync(stadiumId.Value) != null)
            .WithMessage("The stadium does not exist.");
    }

    private static bool BeValidIsoDate(string? value)
    {
        return !string.IsNullOrWhiteSpace(value) && LocalDatePattern.Iso.Parse(value).Success;
    }

    private static bool NotBeInFuture(string? value)
    {
        var parseResult = LocalDatePattern.Iso.Parse(value ?? string.Empty);
        if (!parseResult.Success)
        {
            return true;
        }

        var today = NodaTime.LocalDate.FromDateTime(DateTime.UtcNow.Date);
        return parseResult.Value <= today;
    }
}

public class DeleteClubCommandValidator : AbstractValidator<DeleteClubCommand>
{
    public DeleteClubCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
