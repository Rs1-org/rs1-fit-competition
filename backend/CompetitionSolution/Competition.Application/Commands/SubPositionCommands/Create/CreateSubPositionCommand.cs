using Competition.Application.DTOs.FormationDto;
using Competition.Application.Helpers;
using Competition.Application.Interfaces;
using Competition.Domain.Entities.FormationEntities;
using FluentValidation;
using MediatR;

namespace Competition.Application.Commands.SubPositionCommands.Create;

public class CreateSubPositionCommand : IRequest<SubPositionDto>
{
    public string Name { get; set; } = string.Empty;
    public Guid PositionId { get; set; }
}

public class CreateSubPositionCommandValidator : AbstractValidator<CreateSubPositionCommand>
{
    public CreateSubPositionCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.PositionId).NotEmpty();
    }
}

public class CreateSubPositionCommandHandler : IRequestHandler<CreateSubPositionCommand, SubPositionDto>
{
    private readonly ISubPosition _subPositions;
    private readonly IPosition _positions;
    private readonly IUnitOfWork _unitOfWork;

    public CreateSubPositionCommandHandler(ISubPosition subPositions, IPosition positions, IUnitOfWork unitOfWork)
    {
        _subPositions = subPositions;
        _positions = positions;
        _unitOfWork = unitOfWork;
    }

    public async Task<SubPositionDto> Handle(CreateSubPositionCommand request, CancellationToken cancellationToken)
    {
        var position = await _positions.GetByIdAsync(request.PositionId, cancellationToken)
            ?? throw new KeyNotFoundException($"Position '{request.PositionId}' was not found.");

        var slug = SlugHelper.GenerateSlug($"{position.Name}-{request.Name}");
        if (string.IsNullOrWhiteSpace(slug))
            throw new InvalidOperationException("Subposition name does not produce a valid slug.");
        if (await _subPositions.GetBySlugAsync(slug, cancellationToken) != null)
            throw new InvalidOperationException($"Subposition '{request.Name}' already exists for {position.Name}.");

        var entity = new SubPosition
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Slug = slug,
            PositionId = position.Id,
            Position = position
        };

        await _subPositions.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Map(entity);
    }

    internal static SubPositionDto Map(SubPosition entity) => new()
    {
        Id = entity.Id,
        Name = entity.Name,
        Slug = entity.Slug,
        PositionId = entity.PositionId,
        PositionName = entity.Position?.Name
    };
}
