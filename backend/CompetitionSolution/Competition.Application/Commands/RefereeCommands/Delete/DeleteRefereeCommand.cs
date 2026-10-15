using FluentValidation;
using MediatR;

namespace Competition.Application.Commands.RefereeCommands.Delete;

public class DeleteRefereeCommand(Guid id) : IRequest<Guid>
{
    public Guid Id { get; } = id;
}

public class DeleteRefereeCommandValidator : AbstractValidator<DeleteRefereeCommand>
{
    public DeleteRefereeCommandValidator() => RuleFor(x => x.Id).NotEmpty();
}
