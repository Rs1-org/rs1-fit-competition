using Competition.Application.Common.Validation;
using FluentValidation;
using MediatR;

namespace Competition.Application.Commands.ManagerCommands.Delete;

public class DeleteManagerCommand(string publicId) : IRequest<string>
{
    public string PublicId { get; } = publicId;
}

public class DeleteManagerCommandValidator : AbstractValidator<DeleteManagerCommand>
{
    public DeleteManagerCommandValidator() => RuleFor(x => x.PublicId).MustBeValidPublicId();
}
