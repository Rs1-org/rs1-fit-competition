using Competition.Application.DTOs.FormationDto;
using Competition.Application.Helpers;
using Competition.Application.Interfaces;
using Competition.Domain.Entities.FormationEntities;
using FluentValidation;
using MediatR;

namespace Competition.Application.Commands.PositionCommands.Create;

public class CreatePositionCommand : IRequest<PositionDto>
{
    public string Name { get; set; } = string.Empty;
}

public class CreatePositionCommandValidator : AbstractValidator<CreatePositionCommand>
{
    public CreatePositionCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}

public class CreatePositionCommandHandler : IRequestHandler<CreatePositionCommand, PositionDto>
{
    private readonly IPosition _positions;
    private readonly IUnitOfWork _unitOfWork;

    public CreatePositionCommandHandler(IPosition positions, IUnitOfWork unitOfWork)
    {
        _positions = positions;
        _unitOfWork = unitOfWork;
    }

    public async Task<PositionDto> Handle(CreatePositionCommand request, CancellationToken cancellationToken)
    {
        var slug = SlugHelper.GenerateSlug(request.Name);
        if (string.IsNullOrWhiteSpace(slug))
            throw new InvalidOperationException("Position name does not produce a valid slug.");
        if (await _positions.GetBySlugAsync(slug, cancellationToken) != null)
            throw new InvalidOperationException($"Position '{request.Name}' already exists.");

        var entity = new Position
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Slug = slug
        };

        await _positions.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Map(entity);
    }

    internal static PositionDto Map(Position entity) => new()
    {
        Id = entity.Id,
        Name = entity.Name,
        Slug = entity.Slug
    };
}
