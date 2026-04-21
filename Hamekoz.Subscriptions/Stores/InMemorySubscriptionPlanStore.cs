using Hamekoz.Subscriptions.Models;
using Hamekoz.Subscriptions.Services;

namespace Hamekoz.Subscriptions.Stores;

public sealed class InMemorySubscriptionPlanStore : ISubscriptionPlanStore
{
    private readonly Lock _lock = new();
    private readonly List<SubscriptionPlanDefinition> _plans = [];

    public Task<IReadOnlyList<SubscriptionPlanDefinition>> GetPlansAsync(string applicationId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(applicationId);

        lock (_lock)
        {
            var plans = _plans
                .Where(plan => string.Equals(plan.ApplicationId, applicationId, StringComparison.OrdinalIgnoreCase))
                .ToArray();

            return Task.FromResult<IReadOnlyList<SubscriptionPlanDefinition>>(plans);
        }
    }

    public Task SaveAsync(SubscriptionPlanDefinition plan, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(plan);

        lock (_lock)
        {
            var index = _plans.FindIndex(existing => string.Equals(existing.ApplicationId, plan.ApplicationId, StringComparison.OrdinalIgnoreCase)
                && string.Equals(existing.Id, plan.Id, StringComparison.OrdinalIgnoreCase));

            if (index >= 0)
            {
                _plans[index] = plan;
            }
            else
            {
                _plans.Add(plan);
            }
        }

        return Task.CompletedTask;
    }
}