using AutoMapper;
using Competition.Application.DTOs.RoundDto;
using Competition.Application.Helpers;
using Competition.Application.Interfaces;
using MediatR;

namespace Competition.Application.Commands.RoundCommands.Update;

public class UpdateRoundCommandHandler : IRequestHandler<UpdateRoundCommand, RoundDetailsDto>
{
    private readonly IRound _round;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateRoundCommandHandler(IRound round, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _round = round;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<RoundDetailsDto> Handle(UpdateRoundCommand request, CancellationToken cancellationToken)
    {
        var round = await _round.GetBySlugAsync(request.Slug, cancellationToken)
            ?? throw new KeyNotFoundException($"Round with slug '{request.Slug}' not found.");

        if (await _round.NumberExistsInSeasonAsync(round.SeasonId, request.Number, round.Id, cancellationToken))
            throw BusinessRule.Violation(nameof(request.Number), $"Season {round.Season!.Name} already has round {request.Number}.");

        var slug = SlugHelper.GenerateSlug($"{round.Season!.Slug} round {request.Number}");
        if (await _round.SlugExistsAsync(slug, round.Id, cancellationToken))
            throw BusinessRule.Violation(nameof(request.Number), $"A round with the address '{slug}' already exists.");

        round.Name = request.Name.Trim();
        round.Number = request.Number;
        round.Slug = slug;

        await _round.UpdateAsync(round, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var updated = await _round.GetBySlugAsync(round.Slug, cancellationToken);
        return _mapper.Map<RoundDetailsDto>(updated);
    }
}
