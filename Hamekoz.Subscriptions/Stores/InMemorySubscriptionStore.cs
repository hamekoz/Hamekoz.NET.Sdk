using Hamekoz.Subscriptions.Models;
using Hamekoz.Subscriptions.Services;

namespace Hamekoz.Subscriptions.Stores;

public sealed class InMemorySubscriptionStore : ISubscriptionStore
{
    private readonly Lock _lock = new();
    private readonly List<UserSubscription> _subscriptions = [];

    public Task<IReadOnlyList<UserSubscription>> GetUserSubscriptionsAsync(string applicationId, string userId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(applicationId);
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        lock (_lock)
        {
            var subscriptions = _subscriptions
                .Where(subscription => string.Equals(subscription.ApplicationId, applicationId, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(subscription.UserId, userId, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(subscription => subscription.CreatedAt)
                .ToArray();

            return Task.FromResult<IReadOnlyList<UserSubscription>>(subscriptions);
        }
    }

    public Task<UserSubscription?> GetActiveSubscriptionAsync(string applicationId, string userId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(applicationId);
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        lock (_lock)
        {
            var subscription = _subscriptions
                .Where(subscription => string.Equals(subscription.ApplicationId, applicationId, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(subscription.UserId, userId, StringComparison.OrdinalIgnoreCase)
                    && subscription.IsActive)
                .OrderByDescending(subscription => subscription.ActivatedAt ?? subscription.CreatedAt)
                .FirstOrDefault();

            return Task.FromResult(subscription);
        }
    }

    public Task SaveAsync(UserSubscription subscription, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(subscription);

        lock (_lock)
        {
            var index = _subscriptions.FindIndex(existing => string.Equals(existing.Id, subscription.Id, StringComparison.OrdinalIgnoreCase));
            if (index >= 0)
            {
                _subscriptions[index] = subscription;
            }
            else
            {
                _subscriptions.Add(subscription);
            }
        }

        return Task.CompletedTask;
    }
}
