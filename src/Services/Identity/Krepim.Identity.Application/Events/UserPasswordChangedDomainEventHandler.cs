using Krepim.Identity.Domain.Events;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;

namespace Krepim.Identity.Application.Events
{
    internal sealed class UserPasswordChangedDomainEventHandler(IDistributedCache cache)
        : INotificationHandler<UserPasswordChangedDomainEvent>
    {
        public async Task Handle(UserPasswordChangedDomainEvent notification, CancellationToken cancellationToken)
        {
            string cacheKey = $"jwt-blocklist:{notification.UserId}";

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
            };

            await cache.SetStringAsync(cacheKey, DateTime.UtcNow.ToString(), options, cancellationToken);
        }
    }
}
