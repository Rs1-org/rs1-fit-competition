using Competition.Application.Commands.StadiumCommands.Create;
using Competition.Application.Commands.StadiumCommands.Delete;
using Competition.Application.Commands.StadiumCommands.Update;
using FluentValidation;

namespace Competition.Application.Commands.StadiumCommands.Validators;

public class CreateStadiumCommandValidator : AbstractValidator<CreateStadiumCommand>
{
    public CreateStadiumCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Capacity).GreaterThan(0);
        RuleFor(x => x.CityId).NotEmpty();
        RuleFor(x => x.Latitude).InclusiveBetween(-90, 90);
        RuleFor(x => x.Longitude).InclusiveBetween(-180, 180);
    }
}

public class UpdateStadiumCommandValidator : AbstractValidator<UpdateStadiumCommand>
{
    public UpdateStadiumCommandValidator()
    {
        RuleFor(x => x.Slug).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Capacity).GreaterThan(0);
        RuleFor(x => x.CityId).NotEmpty();
        RuleFor(x => x.Latitude).InclusiveBetween(-90, 90);
        RuleFor(x => x.Longitude).InclusiveBetween(-180, 180);
    }
}

public class DeleteStadiumCommandValidator : AbstractValidator<DeleteStadiumCommand>
{
    public DeleteStadiumCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
