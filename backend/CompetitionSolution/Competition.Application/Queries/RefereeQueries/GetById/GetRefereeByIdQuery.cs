using Competition.Application.Common.Validation;
using Competition.Application.DTOs.PersonDto;
using FluentValidation;
using MediatR;

namespace Competition.Application.Queries.RefereeQueries.GetById;

public class GetRefereeByIdQuery(string publicId) : IRequest<RefereeDto?>
{
    public string PublicId { get; } = publicId;
}

public class GetRefereeByIdQueryValidator : AbstractValidator<GetRefereeByIdQuery>
{
    public GetRefereeByIdQueryValidator() => RuleFor(x => x.PublicId).MustBeValidPublicId();
}
