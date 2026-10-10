using Competition.Application.DTOs.FormationDto;
using Competition.Application.Helpers;
using Competition.Application.Interfaces;
using Competition.Domain.Entities.FormationEntities;
using FluentValidation;
using MediatR;

namespace Competition.Application.Commands.FormationMarkCommands.Create;

public class CreateFormationMarkCommand : IRequest<FormationMarkDto>
{
    public int Defenders { get; set; }
    public int Midfielders { get; set; }
    public int Attackers { get; set; }
}

public class CreateFormationMarkCommandValidator : AbstractValidator<CreateFormationMarkCommand>
{
    public CreateFormationMarkCommandValidator()
    {
        RuleFor(x => x.Defenders).InclusiveBetween(0, 10);
        RuleFor(x => x.Midfielders).InclusiveBetween(0, 10);
        RuleFor(x => x.Attackers).InclusiveBetween(0, 10);
        RuleFor(x => x.Defenders + x.Midfielders + x.Attackers)
            .InclusiveBetween(1, 11)
            .WithMessage("Defenders, midfielders and attackers must add up to between 1 and 11.");
    }
}

public class CreateFormationMarkCommandHandler : IRequestHandler<CreateFormationMarkCommand, FormationMarkDto>
{
    private readonly IFormationMark _marks;
    private readonly IUnitOfWork _unitOfWork;

    public CreateFormationMarkCommandHandler(IFormationMark marks, IUnitOfWork unitOfWork)
    {
        _marks = marks;
        _unitOfWork = unitOfWork;
    }

    public async Task<FormationMarkDto> Handle(CreateFormationMarkCommand request, CancellationToken cancellationToken)
    {
        var slug = SlugHelper.GenerateSlug($"{request.Defenders}-{request.Midfielders}-{request.Attackers}");
        var existing = await _marks.GetBySlugAsync(slug, cancellationToken);
        if (existing != null)
            slug = $"{slug}-{Guid.NewGuid().ToString("N")[..6]}";

        var entity = new FormationMark
        {
            Id = Guid.NewGuid(),
            Defenders = request.Defenders,
            Midfielders = request.Midfielders,
            Attackers = request.Attackers,
            Slug = slug
        };

        await _marks.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Map(entity);
    }

    internal static FormationMarkDto Map(FormationMark entity) => new()
    {
        Id = entity.Id,
        Defenders = entity.Defenders,
        Midfielders = entity.Midfielders,
        Attackers = entity.Attackers,
        Slug = entity.Slug
    };
}
