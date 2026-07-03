using Krepim.SharedKernel.Enums;

namespace Krepim.EventBus.Events.Payment
{
    public sealed record PaymentStatusChangedIntegrationEvent(Guid OrderId, PaymentStatus Status);
}
