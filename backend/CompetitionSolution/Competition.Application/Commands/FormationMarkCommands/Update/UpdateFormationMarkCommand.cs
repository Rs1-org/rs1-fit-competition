using Competition.Application.Commands.FormationMarkCommands.Create;
using Competition.Application.DTOs.FormationDto;
using Competition.Application.Interfaces;
using FluentValidation;
using MediatR;
using System.Text.Json.Serialization;

namespace Competition.Application.Commands.FormationMarkCommands.Update;

public class UpdateFormationMarkCommand : IRequest<FormationMarkDto>
{
    [JsonIgnore]
    public string Slug { get; set; } = string.Empty;
    public int Defenders { get; set; }
    public int Midfielders { get; set; }
    public int Attackers { get; set; }
}

public class UpdateFormationMarkCommandValidator : AbstractValidator<UpdateFormationMarkCommand>
{
    public UpdateFormationMarkCommandValidator()
    {
        RuleFor(x => x.Slug).NotEmpty();
        RuleFor(x => x.Defenders).InclusiveBetween(0, 10);
        RuleFor(x => x.Midfielders).InclusiveBetween(0, 10);
        RuleFor(x => x.Attackers).InclusiveBetween(0, 10);
        RuleFor(x => x.Defenders + x.Midfielders + x.Attackers)
            .InclusiveBetween(1, 11)
            .WithMessage("Defenders, midfielders and attackers must add up to between 1 and 11.");
    }
}

public class UpdateFormationMarkCommandHandler : IRequestHandler<UpdateFormationMarkCommand, FormationMarkDto>
{
    private readonly IFormationMark _marks;
    private readonly IFormation _formations;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateFormationMarkCommandHandler(IFormationMark marks, IFormation formations, IUnitOfWork unitOfWork)
    {
        _marks = marks;
        _formations = formations;
        _unitOfWork = unitOfWork;
    }

    public async Task<FormationMarkDto> Handle(UpdateFormationMarkCommand request, CancellationToken cancellationToken)
    {
        var entity = await _marks.GetBySlugAsync(request.Slug, cancellationToken)
            ?? throw new KeyNotFoundException($"Formation mark '{request.Slug}' was not found.");

        var formation = await _formations.GetByMarkIdAsync(entity.Id, cancellationToken);
        if (formation != null && !Helpers.FormationRules.CodeMatchesMark(formation.Code, request.Defenders, request.Midfielders, request.Attackers))
            throw new InvalidOperationException($"Formation {formation.Code} does not match {request.Defenders}-{request.Midfielders}-{request.Attackers}.");

        entity.Defenders = request.Defenders;
        entity.Midfielders = request.Midfielders;
        entity.Attackers = request.Attackers;

        await _marks.UpdateAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return CreateFormationMarkCommandHandler.Map(entity);
    }
}
