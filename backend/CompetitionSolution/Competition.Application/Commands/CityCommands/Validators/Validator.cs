using Competition.Application.Commands.CityCommands.Create;
using Competition.Application.Commands.CityCommands.Update;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Competition.Application.Commands.CityCommands.Validators
{

    public class CreateCityCommandValidator : AbstractValidator<CreateCityCommand>
    {
        public CreateCityCommandValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Population).GreaterThan(0);
            RuleFor(x => x.CountryId).NotEmpty();
            RuleFor(x => x.Latitude).InclusiveBetween(-90, 90);
            RuleFor(x => x.Longitude).InclusiveBetween(-180, 180);
        }
    }

    public class UpdateCityCommandValidator : AbstractValidator<UpdateCityCommands>
    {
        public UpdateCityCommandValidator()
        {
            RuleFor(x => x.Slug).NotEmpty();
            RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Population).GreaterThan(0);
            RuleFor(x => x.Latitude).InclusiveBetween(-90, 90);
            RuleFor(x => x.Longitude).InclusiveBetween(-180, 180);
        }
    }

    public class DeleteCityCommandValidator : AbstractValidator<DeleteCityCommands>
    {
        public DeleteCityCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
        }
    }
}
