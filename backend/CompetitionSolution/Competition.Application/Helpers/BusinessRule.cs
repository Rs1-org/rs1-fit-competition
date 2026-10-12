using FluentValidation;
using FluentValidation.Results;

namespace Competition.Application.Helpers;

// Rule violations that need the database (uniqueness, existing children) surface as 400 like validator errors.
public static class BusinessRule
{
    public static ValidationException Violation(string property, string message) =>
        new(new[] { new ValidationFailure(property, message) });
}
