using Competition.Application.Interfaces;
using FluentValidation;
using MediatR;

namespace Competition.Application.Commands.FormationMarkCommands.Delete;

public class DeleteFormationMarkCommand : IRequest<Guid>
{
    public Guid Id { get; set; }
}

public class DeleteFormationMarkCommandValidator : AbstractValidator<DeleteFormationMarkCommand>
{
    public DeleteFormationMarkCommandValidator() => RuleFor(x => x.Id).NotEmpty();
}

public class DeleteFormationMarkCommandHandler : IRequestHandler<DeleteFormationMarkCommand, Guid>
{
    private readonly IFormationMark _marks;
    private readonly IFormation _formations;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteFormationMarkCommandHandler(IFormationMark marks, IFormation formations, IUnitOfWork unitOfWork)
    {
        _marks = marks;
        _formations = formations;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(DeleteFormationMarkCommand request, CancellationToken cancellationToken)
    {
        var entity = await _marks.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Formation mark '{request.Id}' was not found.");

        var formation = await _formations.GetByMarkIdAsync(entity.Id, cancellationToken);
        if (formation != null)
            throw new InvalidOperationException("Formation mark is used by a formation and cannot be deleted.");

        await _marks.DeleteAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return request.Id;
    }
}
