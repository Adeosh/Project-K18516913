using FluentAssertions;
using Krepim.SharedKernel.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;

namespace Krepim.SharedKernel.Tests.Extensions
{
    public class GlobalExceptionHandlerTests
    {
        private readonly Mock<ILogger<GlobalExceptionHandler>> _loggerMock;
        private readonly GlobalExceptionHandler _handler;

        public GlobalExceptionHandlerTests()
        {
            _loggerMock = new Mock<ILogger<GlobalExceptionHandler>>();
            _handler = new GlobalExceptionHandler(_loggerMock.Object);
        }

        [Fact]
        public async Task TryHandleAsync_Should_WriteProblemDetailsAndReturnTrue_WhenExceptionOccurs()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var responseStream = new MemoryStream();
            context.Response.Body = responseStream;

            var exception = new InvalidOperationException("Критический сбой бизнес-логики!");

            // Act
            var isHandled = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

            // Assert
            isHandled.Should().BeTrue();
            context.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            context.Response.ContentType.Should().Contain("application/json");

            responseStream.Seek(0, SeekOrigin.Begin);
            var problemDetails = await JsonSerializer.DeserializeAsync<ProblemDetails>(responseStream, cancellationToken: TestContext.Current.CancellationToken);

            problemDetails.Should().NotBeNull();
            problemDetails!.Status.Should().Be(500);
            problemDetails.Title.Should().Be("Server Error");
            problemDetails.Detail.Should().Be("An unexpected error occurred while processing your request.");
            problemDetails.Type.Should().Be("https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1");

            // Проверяем, что логгер зафиксировал ошибку уровня Error
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Unhandled exception occurred")),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
