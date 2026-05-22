using FluentAssertions;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Krepim.EventBus.Tests
{
    public class EventBusHarnessTests
    {
        public record TestIntegrationEvent(Guid AggregateId, string Payload);

        public class TestEventConsumer : IConsumer<TestIntegrationEvent>
        {
            public Task Consume(ConsumeContext<TestIntegrationEvent> context)
            {
                return Task.CompletedTask;
            }
        }

        [Fact]
        public async Task EventBus_Should_PublishAndConsumeMessageSuccessfully()
        {
            // Arrange
            var services = new ServiceCollection();

            services.AddMassTransitTestHarness(x =>
            {
                x.AddConsumer<TestEventConsumer>();
            });

            await using var provider = services.BuildServiceProvider(true);
            var harness = provider.GetRequiredService<ITestHarness>();

            await harness.Start();

            var eventMessage = new TestIntegrationEvent(Guid.NewGuid(), "Hello EventBus");

            // Act
            await harness.Bus.Publish(eventMessage, TestContext.Current.CancellationToken);

            // Assert
            var isPublished = await harness.Published.Any<TestIntegrationEvent>(TestContext.Current.CancellationToken); // публикация
            isPublished.Should().BeTrue("Событие должно быть опубликовано в шину");

            var isConsumed = await harness.Consumed.Any<TestIntegrationEvent>(TestContext.Current.CancellationToken); // потребление
            isConsumed.Should().BeTrue("Событие должно быть доставлено консьюмеру");

            var consumerHarness = harness.GetConsumerHarness<TestEventConsumer>();
            var isProcessedByOurConsumer = await consumerHarness.Consumed.Any<TestIntegrationEvent>(TestContext.Current.CancellationToken);
            isProcessedByOurConsumer.Should().BeTrue("TestEventConsumer должен успешно обработать сообщение");
        }
    }
}
