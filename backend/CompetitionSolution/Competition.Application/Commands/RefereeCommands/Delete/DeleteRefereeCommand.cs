using Competition.Application.Common.Validation;
using FluentValidation;
using MediatR;

namespace Competition.Application.Commands.RefereeCommands.Delete;

public class DeleteRefereeCommand(string publicId) : IRequest<string>
{
    public string PublicId { get; } = publicId;
}

public class DeleteRefereeCommandValidator : AbstractValidator<DeleteRefereeCommand>
{
    public DeleteRefereeCommandValidator() => RuleFor(x => x.PublicId).MustBeValidPublicId();
}
