using Competition.Application.Commands.FormationCommands.Create;
using Competition.Application.DTOs.FormationDto;
using Competition.Application.Helpers;
using Competition.Application.Interfaces;
using FluentValidation;
using MediatR;
using System.Text.Json.Serialization;

namespace Competition.Application.Commands.FormationCommands.Update;

public class UpdateFormationCommand : IRequest<FormationDto>
{
    [JsonIgnore]
    public string Slug { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public Guid FormationMarkId { get; set; }
}

public class UpdateFormationCommandValidator : AbstractValidator<UpdateFormationCommand>
{
    public UpdateFormationCommandValidator()
    {
        RuleFor(x => x.Slug).NotEmpty();
        RuleFor(x => x.Code)
            .NotEmpty()
            .MaximumLength(20)
            .Matches(@"^\d+-\d+-\d+$")
            .WithMessage("Code must look like 4-3-3.");
        RuleFor(x => x.FormationMarkId).NotEmpty();
    }
}

public class UpdateFormationCommandHandler : IRequestHandler<UpdateFormationCommand, FormationDto>
{
    private readonly IFormation _formations;
    private readonly IFormationMark _marks;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateFormationCommandHandler(IFormation formations, IFormationMark marks, IUnitOfWork unitOfWork)
    {
        _formations = formations;
        _marks = marks;
        _unitOfWork = unitOfWork;
    }

    public async Task<FormationDto> Handle(UpdateFormationCommand request, CancellationToken cancellationToken)
    {
        var entity = await _formations.GetBySlugAsync(request.Slug, cancellationToken)
            ?? throw new KeyNotFoundException($"Formation '{request.Slug}' was not found.");

        var mark = await _marks.GetByIdAsync(request.FormationMarkId, cancellationToken)
            ?? throw new KeyNotFoundException($"Formation mark '{request.FormationMarkId}' was not found.");

        if (!FormationRules.CodeMatchesMark(request.Code, mark.Defenders, mark.Midfielders, mark.Attackers))
            throw new InvalidOperationException("Formation code must match the mark (defenders-midfielders-attackers).");

        if (entity.FormationMarkId != mark.Id)
        {
            var taken = await _formations.GetByMarkIdAsync(mark.Id, cancellationToken);
            if (taken != null)
                throw new InvalidOperationException("This formation mark is already linked to a formation.");
        }

        entity.Code = request.Code.Trim();
        entity.FormationMarkId = mark.Id;

        await _formations.UpdateAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var updated = await _formations.GetBySlugAsync(entity.Slug!, cancellationToken);
        return CreateFormationCommandHandler.Map(updated!);
    }
}
