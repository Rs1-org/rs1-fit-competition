using Competition.Application.DTOs.FormationDto;
using Competition.Application.Helpers;
using Competition.Application.Interfaces;
using Competition.Domain.Entities.FormationEntities;
using FluentValidation;
using MediatR;

namespace Competition.Application.Commands.FormationCommands.Create;

public class CreateFormationCommand : IRequest<FormationDto>
{
    public string Code { get; set; } = string.Empty;
    public Guid FormationMarkId { get; set; }
}

public class CreateFormationCommandValidator : AbstractValidator<CreateFormationCommand>
{
    public CreateFormationCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .MaximumLength(20)
            .Matches(@"^\d+-\d+-\d+$")
            .WithMessage("Code must look like 4-3-3.");
        RuleFor(x => x.FormationMarkId).NotEmpty();
    }
}

public class CreateFormationCommandHandler : IRequestHandler<CreateFormationCommand, FormationDto>
{
    private readonly IFormation _formations;
    private readonly IFormationMark _marks;
    private readonly IUnitOfWork _unitOfWork;

    public CreateFormationCommandHandler(IFormation formations, IFormationMark marks, IUnitOfWork unitOfWork)
    {
        _formations = formations;
        _marks = marks;
        _unitOfWork = unitOfWork;
    }

    public async Task<FormationDto> Handle(CreateFormationCommand request, CancellationToken cancellationToken)
    {
        var mark = await _marks.GetByIdAsync(request.FormationMarkId, cancellationToken)
            ?? throw new KeyNotFoundException($"Formation mark '{request.FormationMarkId}' was not found.");

        if (!FormationRules.CodeMatchesMark(request.Code, mark.Defenders, mark.Midfielders, mark.Attackers))
            throw new InvalidOperationException("Formation code must match the mark (defenders-midfielders-attackers).");

        var taken = await _formations.GetByMarkIdAsync(mark.Id, cancellationToken);
        if (taken != null)
            throw new InvalidOperationException("This formation mark is already linked to a formation.");

        var slug = SlugHelper.GenerateSlug(request.Code);
        if (await _formations.GetBySlugAsync(slug, cancellationToken) != null)
            throw new InvalidOperationException($"Formation '{request.Code}' already exists.");

        var entity = new Formation
        {
            Id = Guid.NewGuid(),
            Code = request.Code.Trim(),
            Slug = slug,
            FormationMarkId = mark.Id
        };

        await _formations.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var created = await _formations.GetBySlugAsync(slug, cancellationToken);
        return Map(created!);
    }

    internal static FormationDto Map(Formation entity) => new()
    {
        Id = entity.Id,
        Code = entity.Code,
        Slug = entity.Slug,
        FormationMarkId = entity.FormationMarkId,
        Defenders = entity.FormationMark?.Defenders ?? 0,
        Midfielders = entity.FormationMark?.Midfielders ?? 0,
        Attackers = entity.FormationMark?.Attackers ?? 0
    };
}
