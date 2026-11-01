using Competition.Application.Common.Validation;
using Competition.Application.DTOs.PersonDto;
using FluentValidation;
using MediatR;

namespace Competition.Application.Queries.ManagerQueries.GetById;

public class GetManagerByIdQuery(string publicId) : IRequest<ManagerDto?>
{
    public string PublicId { get; } = publicId;
}

public class GetManagerByIdQueryValidator : AbstractValidator<GetManagerByIdQuery>
{
    public GetManagerByIdQueryValidator() => RuleFor(x => x.PublicId).MustBeValidPublicId();
}
