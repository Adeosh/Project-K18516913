using FluentAssertions;
using Krepim.Payment.Domain.Entities;
using Krepim.SharedKernel.Enums;
using Krepim.SharedKernel.Results;

namespace Krepim.Payment.Domain.Tests.Entities
{
    public class PaymentTransactionTests
    {
        [Fact]
        public void Create_Should_ReturnSuccess_When_AmountIsGreaterThanZero()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var amount = 1500.00m;

            // Act
            var result = PaymentTransaction.Create(orderId, amount);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.OrderId.Should().Be(orderId);
            result.Value.Amount.Should().Be(amount);
            result.Value.Status.Should().Be(PaymentStatus.Pending);
            result.Value.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-100)]
        public void Create_Should_ReturnFailure_When_AmountIsZeroOrNegative(decimal invalidAmount)
        {
            // Arrange
            var orderId = Guid.NewGuid();

            // Act
            var result = PaymentTransaction.Create(orderId, invalidAmount);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("Payment.InvalidAmount");
            result.Error.Type.Should().Be(ErrorType.Validation);
        }

        [Fact]
        public void SetExternalDetails_Should_UpdateFields_When_ParamsAreValid()
        {
            // Arrange
            var transaction = PaymentTransaction.Create(Guid.NewGuid(), 500m).Value;
            var externalId = "ch_3MtwfvLkdIwHu7ix28a3";
            var paymentUrl = "https://checkout.stripe.com/pay/test";

            // Act
            transaction.SetExternalDetails(externalId, paymentUrl);

            // Assert
            transaction.ExternalPaymentId.Should().Be(externalId);
            transaction.PaymentUrl.Should().Be(paymentUrl);
        }

        [Theory]
        [InlineData("", "https://stripe.com")]
        [InlineData("ch_123", "")]
        [InlineData(" ", "   ")]
        public void SetExternalDetails_Should_ThrowArgumentException_When_ParamsAreNullOrWhitespace(string extId, string url)
        {
            // Arrange
            var transaction = PaymentTransaction.Create(Guid.NewGuid(), 500m).Value;

            // Act
            Action act = () => transaction.SetExternalDetails(extId, url);

            // Assert
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void MarkAsSucceeded_Should_SetStatusAndProcessedAt_When_StatusIsPending()
        {
            // Arrange
            var transaction = PaymentTransaction.Create(Guid.NewGuid(), 1000m).Value;

            // Act
            var result = transaction.MarkAsSucceeded();

            // Assert
            result.IsSuccess.Should().BeTrue();
            transaction.Status.Should().Be(PaymentStatus.Succeeded);
            transaction.ProcessedAt.Should().NotBeNull();
            transaction.ProcessedAt!.Value.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void MarkAsSucceeded_Should_ReturnSuccessIdempotently_When_AlreadySucceeded()
        {
            // Arrange
            var transaction = PaymentTransaction.Create(Guid.NewGuid(), 1000m).Value;
            transaction.MarkAsSucceeded();

            // Act
            var result = transaction.MarkAsSucceeded();

            // Assert
            result.IsSuccess.Should().BeTrue();
            transaction.Status.Should().Be(PaymentStatus.Succeeded);
        }

        [Fact]
        public void MarkAsFailed_Should_SetStatusAndErrorMessage_When_StatusIsPending()
        {
            // Arrange
            var transaction = PaymentTransaction.Create(Guid.NewGuid(), 1000m).Value;
            var reason = "Card Insufficient Funds";

            // Act
            var result = transaction.MarkAsFailed(reason);

            // Assert
            result.IsSuccess.Should().BeTrue();
            transaction.Status.Should().Be(PaymentStatus.Failed);
            transaction.ErrorMessage.Should().Be(reason);
            transaction.ProcessedAt.Should().NotBeNull();
        }

        [Fact]
        public void MarkAsSucceeded_And_MarkAsFailed_Should_ReturnFailureConflict_When_StatusIsNotPending()
        {
            // Arrange
            var transaction = PaymentTransaction.Create(Guid.NewGuid(), 1000m).Value;
            transaction.MarkAsFailed("Initial failure");

            // Act
            var successResult = transaction.MarkAsSucceeded();
            var failureResult = transaction.MarkAsFailed("Another error");

            // Assert
            successResult.IsFailure.Should().BeTrue();
            successResult.Error.Code.Should().Be("Payment.InvalidState");
            successResult.Error.Type.Should().Be(ErrorType.Conflict);

            failureResult.IsFailure.Should().BeTrue();
            failureResult.Error.Code.Should().Be("Payment.InvalidState");
            failureResult.Error.Type.Should().Be(ErrorType.Conflict);
        }

        [Fact]
        public void MarkAsRefunded_Should_SetStatus_When_StatusIsSucceeded()
        {
            // Arrange
            var transaction = PaymentTransaction.Create(Guid.NewGuid(), 1000m).Value;
            transaction.MarkAsSucceeded();

            // Act
            var result = transaction.MarkAsRefunded();

            // Assert
            result.IsSuccess.Should().BeTrue();
            transaction.Status.Should().Be(PaymentStatus.Refunded);
            transaction.ProcessedAt.Should().NotBeNull();
        }

        [Fact]
        public void MarkAsRefunded_Should_ReturnFailure_When_StatusIsNotSucceeded()
        {
            // Arrange
            var transaction = PaymentTransaction.Create(Guid.NewGuid(), 1000m).Value;

            // Act
            var result = transaction.MarkAsRefunded();

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("Payment.InvalidState");
        }

        [Theory]
        [InlineData(PaymentStatus.Succeeded, true)]
        [InlineData(PaymentStatus.Failed, true)]
        [InlineData(PaymentStatus.Refunded, false)]
        public void HandleStatus_Should_ReturnExpectedResult_When_StatusIsPending(PaymentStatus status, bool expectedSuccess)
        {
            // Arrange
            var transaction = PaymentTransaction.Create(Guid.NewGuid(), 1000m).Value;

            // Act
            var result = transaction.HandleStatus(status, "Test Error");

            // Assert
            result.IsSuccess.Should().Be(expectedSuccess);
            if (expectedSuccess)
            {
                transaction.Status.Should().Be(status);
            }
        }

        [Fact]
        public void HandleStatus_Should_ReturnConflict_When_TransactionIsAlreadyFinished()
        {
            // Arrange
            var transaction = PaymentTransaction.Create(Guid.NewGuid(), 1000m).Value;
            transaction.MarkAsSucceeded(); // Завершили

            // Act
            var result = transaction.HandleStatus(PaymentStatus.Failed);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("Payment.InvalidState");
        }
    }
}
