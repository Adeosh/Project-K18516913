namespace Krepim.EventBus.Events.Payment
{
    public sealed record PaymentSucceededIntegrationEvent(Guid OrderId);
}
