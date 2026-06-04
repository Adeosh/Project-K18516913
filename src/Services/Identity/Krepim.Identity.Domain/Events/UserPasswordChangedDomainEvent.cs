using Krepim.SharedKernel.Domain.Abstractions;

namespace Krepim.Identity.Domain.Events
{
    public sealed record UserPasswordChangedDomainEvent(Guid UserId) : IDomainEvent;
}
