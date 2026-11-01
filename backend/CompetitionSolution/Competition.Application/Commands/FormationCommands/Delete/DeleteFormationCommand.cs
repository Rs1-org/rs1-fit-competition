using Competition.Application.Common.Validation;
using Competition.Application.Interfaces;
using Competition.Domain.Entities.FormationEntities;
using FluentValidation;
using MediatR;

namespace Competition.Application.Commands.FormationCommands.Delete;

public class DeleteFormationCommand : IRequest<string>
{
    public string PublicId { get; set; } = string.Empty;
}

public class DeleteFormationCommandValidator : AbstractValidator<DeleteFormationCommand>
{
    public DeleteFormationCommandValidator() => RuleFor(x => x.PublicId).MustBeValidPublicId();
}

public class DeleteFormationCommandHandler : IRequestHandler<DeleteFormationCommand, string>
{
    private readonly IFormation _formations;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPublicIdLookup _lookup;

    public DeleteFormationCommandHandler(IFormation formations, IUnitOfWork unitOfWork, IPublicIdLookup lookup)
    {
        _formations = formations;
        _unitOfWork = unitOfWork;
        _lookup = lookup;
    }

    public async Task<string> Handle(DeleteFormationCommand request, CancellationToken cancellationToken)
    {
        var id = await _lookup.GetRequiredIdAsync<Formation>(request.PublicId, cancellationToken);
        var entity = await _formations.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Formation '{request.PublicId}' was not found.");

        await _formations.DeleteAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return entity.PublicId;
    }
}
