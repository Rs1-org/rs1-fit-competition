using Competition.Application.Common.Validation;
using Competition.Application.Interfaces;
using Competition.Domain.Entities;
using FluentValidation;
using MediatR;

namespace Competition.Application.Commands.SelectionCommands.Delete;

public class DeleteSelectionCommand : IRequest<string>
{
    public string PublicId { get; set; } = string.Empty;
}

public class DeleteSelectionCommandValidator : AbstractValidator<DeleteSelectionCommand>
{
    public DeleteSelectionCommandValidator() => RuleFor(x => x.PublicId).MustBeValidPublicId();
}

public class DeleteSelectionCommandHandler : IRequestHandler<DeleteSelectionCommand, string>
{
    private readonly ISelection _selections;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPublicIdLookup _lookup;

    public DeleteSelectionCommandHandler(ISelection selections, IUnitOfWork unitOfWork, IPublicIdLookup lookup)
    {
        _selections = selections;
        _unitOfWork = unitOfWork;
        _lookup = lookup;
    }

    public async Task<string> Handle(DeleteSelectionCommand request, CancellationToken cancellationToken)
    {
        var id = await _lookup.GetRequiredIdAsync<Selection>(request.PublicId, cancellationToken);
        var entity = await _selections.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Selection '{request.PublicId}' was not found.");

        await _selections.DeleteAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return entity.PublicId;
    }
}
