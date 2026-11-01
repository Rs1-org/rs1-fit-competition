using Competition.Application.Common.Validation;
using Competition.Application.DTOs.PersonDto;
using FluentValidation;
using MediatR;

namespace Competition.Application.Queries.PlayerQueries.GetById;

public class GetPlayerByIdQuery(string publicId) : IRequest<PlayerDto?>
{
    public string PublicId { get; } = publicId;
}

public class GetPlayerByIdQueryValidator : AbstractValidator<GetPlayerByIdQuery>
{
    public GetPlayerByIdQueryValidator() => RuleFor(x => x.PublicId).MustBeValidPublicId();
}
