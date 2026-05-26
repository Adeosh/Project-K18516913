using FluentAssertions;
using Krepim.SharedKernel.Results;
using Krepim.SharedKernel.Results.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;

namespace Krepim.SharedKernel.Tests.Results
{
    public class ResultEndpointFilterTests
    {
        private readonly ResultEndpointFilter _filter;
        private readonly Mock<HttpContext> _httpContextMock;

        public ResultEndpointFilterTests()
        {
            _filter = new ResultEndpointFilter();
            _httpContextMock = new Mock<HttpContext>();
        }

        [Fact]
        public async Task InvokeAsync_Should_ReturnOkResultWithData_When_DelegateReturnsGenericSuccessResult()
        {
            // Arrange
            var domainResult = Result<string>.Success("Успешные данные каталога");

            var context = new DefaultEndpointFilterInvocationContext(_httpContextMock.Object, domainResult);

            EndpointFilterDelegate nextDelegate = (ctx) => ValueTask.FromResult<object?>(domainResult);

            // Act
            var output = await _filter.InvokeAsync(context, nextDelegate);

            // Assert
            output.Should().BeOfType<Ok<object>>();
            var okResult = (Ok<object>)output!;
            okResult.Value.Should().Be("Успешные данные каталога");
        }

        [Fact]
        public async Task InvokeAsync_Should_ReturnNoContent_When_DelegateReturnsNonGenericSuccessResult()
        {
            // Arrange
            var domainResult = Result.Success();
            var context = new DefaultEndpointFilterInvocationContext(_httpContextMock.Object, domainResult);
            EndpointFilterDelegate nextDelegate = (ctx) => ValueTask.FromResult<object?>(domainResult);

            // Act
            var output = await _filter.InvokeAsync(context, nextDelegate);

            // Assert
            output.Should().BeOfType<NoContent>();
        }

        [Fact]
        public async Task InvokeAsync_Should_ReturnProblemDetails_When_DelegateReturnsFailureResult()
        {
            // Arrange
            var error = new Error("Product.NotFound", "Товар не обнаружен", ErrorType.NotFound);
            var domainResult = Result<Guid>.Failure(error);

            var context = new DefaultEndpointFilterInvocationContext(_httpContextMock.Object, domainResult);
            EndpointFilterDelegate nextDelegate = (ctx) => ValueTask.FromResult<object?>(domainResult);

            // Act
            var output = await _filter.InvokeAsync(context, nextDelegate);

            // Assert
            output.Should().NotBeNull();
            output!.GetType().Name.Should().Contain("ProblemHttpResult");
        }

        [Fact]
        public async Task InvokeAsync_Should_PassThrough_When_DelegateReturnsStandardIResult()
        {
            // Arrange
            var standardHttpResult = Microsoft.AspNetCore.Http.Results.Created("/api/test", 123);
            var context = new DefaultEndpointFilterInvocationContext(_httpContextMock.Object, standardHttpResult);
            EndpointFilterDelegate nextDelegate = (ctx) => ValueTask.FromResult<object?>(standardHttpResult);

            // Act
            var output = await _filter.InvokeAsync(context, nextDelegate);

            // Assert
            output.Should().BeSameAs(standardHttpResult);
        }
    }
}
