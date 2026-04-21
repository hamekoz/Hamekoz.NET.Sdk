using Hamekoz.Subscriptions.Models;

namespace Hamekoz.Subscriptions.Services;

public interface ISubscriptionPlanStore
{
    Task<IReadOnlyList<SubscriptionPlanDefinition>> GetPlansAsync(string applicationId, CancellationToken cancellationToken = default);

    Task SaveAsync(SubscriptionPlanDefinition plan, CancellationToken cancellationToken = default);
}