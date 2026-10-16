using AutoMapper;
using Competition.Application.DTOs.RoundDto;
using Competition.Application.Helpers;
using Competition.Application.Interfaces;
using Competition.Domain.Entities.CompetitionEntities;
using MediatR;

namespace Competition.Application.Commands.RoundCommands.Create;

public class CreateRoundCommandHandler : IRequestHandler<CreateRoundCommand, RoundDetailsDto>
{
    private readonly IRound _round;
    private readonly IPublicIdLookup _lookup;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateRoundCommandHandler(IRound round, IPublicIdLookup lookup, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _round = round;
        _lookup = lookup;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<RoundDetailsDto> Handle(CreateRoundCommand request, CancellationToken cancellationToken)
    {
        var season = await _lookup.GetRequiredEntityAsync<Season>(request.SeasonPublicId, cancellationToken);

        if (await _round.NumberExistsInSeasonAsync(season.Id, request.Number, null, cancellationToken))
            throw BusinessRule.Violation(nameof(request.Number), $"Season {season.Name} already has round {request.Number}.");

        var slug = SlugHelper.GenerateSlug($"{season.Slug} round {request.Number}");
        if (await _round.SlugExistsAsync(slug, null, cancellationToken))
            throw BusinessRule.Violation(nameof(request.Number), $"A round with the address '{slug}' already exists.");

        var round = new Round
        {
            SeasonId = season.Id,
            Name = request.Name.Trim(),
            Number = request.Number,
            Slug = slug
        };

        await _round.AddAsync(round, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var created = await _round.GetBySlugAsync(round.Slug, cancellationToken);
        return _mapper.Map<RoundDetailsDto>(created);
    }
}
