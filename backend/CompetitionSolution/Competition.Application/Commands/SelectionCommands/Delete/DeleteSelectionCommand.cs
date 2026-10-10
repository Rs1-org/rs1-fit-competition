using Competition.Application.Interfaces;
using FluentValidation;
using MediatR;

namespace Competition.Application.Commands.SelectionCommands.Delete;

public class DeleteSelectionCommand : IRequest<Guid>
{
    public Guid Id { get; set; }
}

public class DeleteSelectionCommandValidator : AbstractValidator<DeleteSelectionCommand>
{
    public DeleteSelectionCommandValidator() => RuleFor(x => x.Id).NotEmpty();
}

public class DeleteSelectionCommandHandler : IRequestHandler<DeleteSelectionCommand, Guid>
{
    private readonly ISelection _selections;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteSelectionCommandHandler(ISelection selections, IUnitOfWork unitOfWork)
    {
        _selections = selections;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(DeleteSelectionCommand request, CancellationToken cancellationToken)
    {
        var entity = await _selections.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Selection '{request.Id}' was not found.");

        await _selections.DeleteAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return request.Id;
    }
}
