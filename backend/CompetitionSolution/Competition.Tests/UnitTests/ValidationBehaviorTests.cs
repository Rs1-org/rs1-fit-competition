using Competition.Application.Behaviors;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Moq;
using Xunit;

namespace Competition.Tests.UnitTests;

public class ValidationBehaviorTests
{
    private readonly Mock<RequestHandlerDelegate<string>> _nextMock;

    public ValidationBehaviorTests()
    {
        _nextMock = new Mock<RequestHandlerDelegate<string>>();
        _nextMock.Setup(x => x(It.IsAny<CancellationToken>())).ReturnsAsync("Success");
    }

    [Fact]
    public async Task Handle_NoValidators_ShouldCallNext()
    {
        // Arrange
        var behavior = new ValidationBehavior<IRequest<string>, string>(Enumerable.Empty<IValidator<IRequest<string>>>());
        var request = new Mock<IRequest<string>>().Object;

        // Act
        var result = await behavior.Handle(request, _nextMock.Object, CancellationToken.None);

        // Assert
        result.Should().Be("Success");
        _nextMock.Verify(x => x(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidRequest_ShouldCallNext()
    {
        // Arrange
        var validatorMock = new Mock<IValidator<IRequest<string>>>();
        
        // Setup using a match that avoids optional param issues
        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<IRequest<string>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var behavior = new ValidationBehavior<IRequest<string>, string>(new[] { validatorMock.Object });
        var request = new Mock<IRequest<string>>().Object;

        // Act
        var result = await behavior.Handle(request, _nextMock.Object, CancellationToken.None);

        // Assert
        result.Should().Be("Success");
    }

    [Fact]
    public async Task Handle_InvalidRequest_ShouldThrowValidationException()
    {
        // Arrange
        var failure = new ValidationFailure("Prop", "Error");
        var validatorMock = new Mock<IValidator<IRequest<string>>>();
        
        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<IRequest<string>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(new[] { failure }));

        var behavior = new ValidationBehavior<IRequest<string>, string>(new[] { validatorMock.Object });
        var request = new Mock<IRequest<string>>().Object;

        // Act
        var act = async () => await behavior.Handle(request, _nextMock.Object, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
    }
}
