using FluentAssertions;
using Krepim.SharedKernel.Results;

namespace Krepim.SharedKernel.Tests.Results
{
    public class ResultTests
    {
        [Fact]
        public void Success_Should_ReturnIsSuccessTrue_And_ErrorNone()
        {
            // Act
            var result = Result.Success();

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.IsFailure.Should().BeFalse();
            result.Error.Should().Be(Error.None);
        }

        [Fact]
        public void Failure_Should_ReturnIsFailureTrue_And_CorrectError()
        {
            // Arrange
            var error = new Error("Test.Error", "Test description", ErrorType.Validation);

            // Act
            var result = Result.Failure(error);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be(error);
        }

        [Fact]
        public void ResultT_Value_Should_ReturnProvidedValue_WhenSuccess()
        {
            // Arrange
            var expectedValue = Guid.NewGuid();

            // Act
            var result = Result<Guid>.Success(expectedValue);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(expectedValue);
        }

        [Fact]
        public void ResultT_Value_Should_ThrowInvalidOperationException_WhenFailure()
        {
            // Arrange
            var error = new Error("Test.Fail", "Fail", ErrorType.Failure);
            var result = Result<Guid>.Failure(error);

            // Act
            var action = () => result.Value;

            // Assert
            action.Should().Throw<InvalidOperationException>()
                .WithMessage($"The value of a failure result can't be accessed. Error: {error.Code}");
        }

        [Fact]
        public void ResultT_ImplicitOperator_Should_ConvertValueToSuccessResult()
        {
            // Arrange
            var expectedValue = "Hello World";

            // Act
            Result<string> result = expectedValue;

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(expectedValue);
        }
    }
}
