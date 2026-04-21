using Hamekoz.Subscriptions.Models;

namespace Hamekoz.Subscriptions.Services;

public interface ISubscriptionCatalogService
{
    Task<IReadOnlyList<SubscriptionPlanDefinition>> GetPlansAsync(string applicationId, CancellationToken cancellationToken = default);

    Task<SubscriptionPlanDefinition> GetPlanAsync(string applicationId, string planId, CancellationToken cancellationToken = default);

    Task<SubscriptionPlanDefinition> SavePlanAsync(SubscriptionPlanDefinition plan, CancellationToken cancellationToken = default);

    Task<SubscriptionPlanDefinition> EnsureDefaultFreePlanAsync(string applicationId, CancellationToken cancellationToken = default);
}