using Hamekoz.Subscriptions.Models;

namespace Hamekoz.Subscriptions.Services;

public interface ISubscriptionStore
{
    Task<IReadOnlyList<UserSubscription>> GetUserSubscriptionsAsync(string applicationId, string userId, CancellationToken cancellationToken = default);

    Task<UserSubscription?> GetActiveSubscriptionAsync(string applicationId, string userId, CancellationToken cancellationToken = default);

    Task SaveAsync(UserSubscription subscription, CancellationToken cancellationToken = default);
}