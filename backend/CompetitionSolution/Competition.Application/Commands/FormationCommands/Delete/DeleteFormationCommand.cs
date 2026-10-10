using Competition.Application.Interfaces;
using FluentValidation;
using MediatR;

namespace Competition.Application.Commands.FormationCommands.Delete;

public class DeleteFormationCommand : IRequest<Guid>
{
    public Guid Id { get; set; }
}

public class DeleteFormationCommandValidator : AbstractValidator<DeleteFormationCommand>
{
    public DeleteFormationCommandValidator() => RuleFor(x => x.Id).NotEmpty();
}

public class DeleteFormationCommandHandler : IRequestHandler<DeleteFormationCommand, Guid>
{
    private readonly IFormation _formations;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteFormationCommandHandler(IFormation formations, IUnitOfWork unitOfWork)
    {
        _formations = formations;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(DeleteFormationCommand request, CancellationToken cancellationToken)
    {
        var entity = await _formations.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Formation '{request.Id}' was not found.");

        await _formations.DeleteAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return request.Id;
    }
}
