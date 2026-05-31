using AutoMapper;
using Competition.Application.DTOs.MatchDto;
using Competition.Application.Interfaces;
using Competition.Domain.Entities.MatchEntities;
using MediatR;
using NodaTime;

namespace Competition.Application.Commands.MatchCommands.Create;

public class CreateMatchCommandHandler : IRequestHandler<CreateMatchCommand, MatchDetailsDto>
{
    private readonly IMatch _match;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateMatchCommandHandler(IMatch match, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _match = match;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<MatchDetailsDto> Handle(CreateMatchCommand request, CancellationToken cancellationToken)
    {
        var match = _mapper.Map<Match>(request);
        match.Date = Instant.FromDateTimeUtc(DateTime.SpecifyKind(request.Date, DateTimeKind.Utc));

        await _match.AddAsync(match, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var created = await _match.GetBySlugAsync(match.Slug!, cancellationToken);
        return _mapper.Map<MatchDetailsDto>(created);
    }
}
