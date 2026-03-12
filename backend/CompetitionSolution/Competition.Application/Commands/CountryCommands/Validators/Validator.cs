using Competition.Application.Commands.CountryCommands.Create;
using Competition.Application.Commands.CountryCommands.Delete;
using Competition.Application.Commands.CountryCommands.Update;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Competition.Application.Commands.CountryCommands.Validators
{
    public class CreateCountryCommandValidator : AbstractValidator<CreateCountryCommands>
    {
        public CreateCountryCommandValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        }
    }

    public class UpdateCountryCommandValidator : AbstractValidator<UpdateCountryCommands>
    {
        public UpdateCountryCommandValidator()
        {
            RuleFor(x => x.Slug).NotEmpty();
            RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        }
    }

    public class DeleteCountryCommandValidator : AbstractValidator<DeleteCountryCommands>
    {
        public DeleteCountryCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
        }
    }
}
