using Competition.Application.Common.Validation;
using Competition.Application.Interfaces;
using Competition.Domain.Entities.FormationEntities;
using FluentValidation;
using MediatR;

namespace Competition.Application.Commands.FormationMarkCommands.Delete;

public class DeleteFormationMarkCommand : IRequest<string>
{
    public string PublicId { get; set; } = string.Empty;
}

public class DeleteFormationMarkCommandValidator : AbstractValidator<DeleteFormationMarkCommand>
{
    public DeleteFormationMarkCommandValidator() => RuleFor(x => x.PublicId).MustBeValidPublicId();
}

public class DeleteFormationMarkCommandHandler : IRequestHandler<DeleteFormationMarkCommand, string>
{
    private readonly IFormationMark _marks;
    private readonly IFormation _formations;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPublicIdLookup _lookup;

    public DeleteFormationMarkCommandHandler(IFormationMark marks, IFormation formations, IUnitOfWork unitOfWork, IPublicIdLookup lookup)
    {
        _marks = marks;
        _formations = formations;
        _unitOfWork = unitOfWork;
        _lookup = lookup;
    }

    public async Task<string> Handle(DeleteFormationMarkCommand request, CancellationToken cancellationToken)
    {
        var id = await _lookup.GetRequiredIdAsync<FormationMark>(request.PublicId, cancellationToken);
        var entity = await _marks.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Formation mark '{request.PublicId}' was not found.");

        var formation = await _formations.GetByMarkIdAsync(entity.Id, cancellationToken);
        if (formation != null)
            throw new InvalidOperationException("Formation mark is used by a formation and cannot be deleted.");

        await _marks.DeleteAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return entity.PublicId;
    }
}
