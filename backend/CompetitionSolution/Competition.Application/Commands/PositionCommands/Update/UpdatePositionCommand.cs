using Competition.Application.Commands.PositionCommands.Create;
using Competition.Application.DTOs.FormationDto;
using Competition.Application.Interfaces;
using FluentValidation;
using MediatR;
using System.Text.Json.Serialization;

namespace Competition.Application.Commands.PositionCommands.Update;

public class UpdatePositionCommand : IRequest<PositionDto>
{
    [JsonIgnore]
    public string Slug { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public class UpdatePositionCommandValidator : AbstractValidator<UpdatePositionCommand>
{
    public UpdatePositionCommandValidator()
    {
        RuleFor(x => x.Slug).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}

public class UpdatePositionCommandHandler : IRequestHandler<UpdatePositionCommand, PositionDto>
{
    private readonly IPosition _positions;
    private readonly IUnitOfWork _unitOfWork;

    public UpdatePositionCommandHandler(IPosition positions, IUnitOfWork unitOfWork)
    {
        _positions = positions;
        _unitOfWork = unitOfWork;
    }

    public async Task<PositionDto> Handle(UpdatePositionCommand request, CancellationToken cancellationToken)
    {
        var entity = await _positions.GetBySlugAsync(request.Slug, cancellationToken)
            ?? throw new KeyNotFoundException($"Position '{request.Slug}' was not found.");

        entity.Name = request.Name.Trim();
        await _positions.UpdateAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return CreatePositionCommandHandler.Map(entity);
    }
}
