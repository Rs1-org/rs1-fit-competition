using FluentValidation;
using MediatR;

namespace Competition.Application.Features.PlayerCommands.Delete;

public class DeletePlayerCommand(Guid id) : IRequest<Guid>
{
    public Guid Id { get; } = id;
}

public class DeletePlayerCommandValidator : AbstractValidator<DeletePlayerCommand>
{
    public DeletePlayerCommandValidator() => RuleFor(x => x.Id).NotEmpty();
}
