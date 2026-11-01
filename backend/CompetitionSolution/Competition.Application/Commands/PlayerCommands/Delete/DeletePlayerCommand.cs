using Competition.Application.Common.Validation;
using FluentValidation;
using MediatR;

namespace Competition.Application.Commands.PlayerCommands.Delete;

public class DeletePlayerCommand(string publicId) : IRequest<string>
{
    public string PublicId { get; } = publicId;
}

public class DeletePlayerCommandValidator : AbstractValidator<DeletePlayerCommand>
{
    public DeletePlayerCommandValidator() => RuleFor(x => x.PublicId).MustBeValidPublicId();
}
