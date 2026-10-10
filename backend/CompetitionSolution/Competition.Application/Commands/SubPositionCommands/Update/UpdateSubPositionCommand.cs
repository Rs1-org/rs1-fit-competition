using Competition.Application.Commands.SubPositionCommands.Create;
using Competition.Application.DTOs.FormationDto;
using Competition.Application.Interfaces;
using FluentValidation;
using MediatR;
using System.Text.Json.Serialization;

namespace Competition.Application.Commands.SubPositionCommands.Update;

public class UpdateSubPositionCommand : IRequest<SubPositionDto>
{
    [JsonIgnore]
    public string Slug { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public Guid PositionId { get; set; }
}

public class UpdateSubPositionCommandValidator : AbstractValidator<UpdateSubPositionCommand>
{
    public UpdateSubPositionCommandValidator()
    {
        RuleFor(x => x.Slug).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.PositionId).NotEmpty();
    }
}

public class UpdateSubPositionCommandHandler : IRequestHandler<UpdateSubPositionCommand, SubPositionDto>
{
    private readonly ISubPosition _subPositions;
    private readonly IPosition _positions;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateSubPositionCommandHandler(ISubPosition subPositions, IPosition positions, IUnitOfWork unitOfWork)
    {
        _subPositions = subPositions;
        _positions = positions;
        _unitOfWork = unitOfWork;
    }

    public async Task<SubPositionDto> Handle(UpdateSubPositionCommand request, CancellationToken cancellationToken)
    {
        var entity = await _subPositions.GetBySlugAsync(request.Slug, cancellationToken)
            ?? throw new KeyNotFoundException($"Subposition '{request.Slug}' was not found.");

        var position = await _positions.GetByIdAsync(request.PositionId, cancellationToken)
            ?? throw new KeyNotFoundException($"Position '{request.PositionId}' was not found.");

        if (entity.PositionId != position.Id && await _subPositions.HasPlayersAsync(entity.Id, cancellationToken))
            throw new InvalidOperationException("Subposition is assigned to players and its parent position cannot change.");

        entity.Name = request.Name.Trim();
        entity.PositionId = position.Id;
        entity.Position = position;

        await _subPositions.UpdateAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return CreateSubPositionCommandHandler.Map(entity);
    }
}
