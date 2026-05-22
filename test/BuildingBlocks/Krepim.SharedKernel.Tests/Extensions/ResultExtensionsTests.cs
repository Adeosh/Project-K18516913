using FluentAssertions;
using Krepim.SharedKernel.Extensions;
using Krepim.SharedKernel.Results;

namespace Krepim.SharedKernel.Tests.Extensions
{
    public class ResultExtensionsTests
    {
        [Fact]
        public void Match_Should_ExecuteSuccessDelegate_WhenResultIsSuccess()
        {
            // Arrange
            var result = Result<string>.Success("Data");

            var successResult = Microsoft.AspNetCore.Http.Results.Ok("Data");

            // Act
            var output = result.Match(
                onSuccess: val => successResult
            );

            // Assert
            output.Should().BeSameAs(successResult);
        }

        [Fact]
        public void Match_Should_ReturnProblemDetails_WhenResultIsFailure()
        {
            // Arrange
            var error = new Error("Test.Error", "Desc", ErrorType.NotFound);
            var result = Result<string>.Failure(error);

            // Act
            var output = result.Match(
                onSuccess: val => Microsoft.AspNetCore.Http.Results.Ok(val)
            );

            // Assert
            output.Should().NotBeNull();

            var isProblemDetails = output.GetType().Name.Contains("ProblemHttpResult");
            isProblemDetails.Should().BeTrue();
        }
    }
}
