using Hamekoz.Features.Models;
using Hamekoz.Subscriptions.Models;

namespace Hamekoz.Subscriptions.Services;

public sealed class SubscriptionService(
    ISubscriptionCatalogService subscriptionCatalogService,
    ISubscriptionPlanStore planStore,
    ISubscriptionStore subscriptionStore) : ISubscriptionService
{
    private readonly ISubscriptionCatalogService _subscriptionCatalogService = subscriptionCatalogService;
    private readonly ISubscriptionPlanStore _planStore = planStore;
    private readonly ISubscriptionStore _subscriptionStore = subscriptionStore;

    public async Task<UserSubscription> EnsureActiveSubscriptionAsync(string applicationId, string userId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(applicationId);
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        var activeSubscription = await _subscriptionStore.GetActiveSubscriptionAsync(applicationId, userId, cancellationToken);
        if (activeSubscription is not null)
        {
            return activeSubscription;
        }

        var freePlan = await _subscriptionCatalogService.EnsureDefaultFreePlanAsync(applicationId, cancellationToken);
        var subscription = new UserSubscription
        {
            ApplicationId = applicationId,
            UserId = userId,
            PlanId = freePlan.Id,
            Period = SubscriptionPeriod.Permanent,
            Status = SubscriptionStatus.Active,
            ActivatedAt = DateTime.UtcNow,
            ExpiresAt = null,
            AutoRenew = false
        };

        await _subscriptionStore.SaveAsync(subscription, cancellationToken);
        return subscription;
    }

    public async Task<UserSubscription> AssignPlanAsync(
        string applicationId,
        string userId,
        string planId,
        SubscriptionPeriod period,
        bool autoRenew = false,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(applicationId);
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(planId);

        await _subscriptionCatalogService.EnsureDefaultFreePlanAsync(applicationId, cancellationToken);

        var plans = await _planStore.GetPlansAsync(applicationId, cancellationToken);
        var plan = plans.FirstOrDefault(existing => string.Equals(existing.Id, planId, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException($"No se encontro el plan '{planId}' para la aplicacion '{applicationId}'.");

        if (!plan.IsActive)
        {
            throw new InvalidOperationException($"El plan '{planId}' no se encuentra activo.");
        }

        if (!plan.AvailablePeriods.Contains(period))
        {
            throw new InvalidOperationException($"El plan '{planId}' no soporta el periodo '{period}'.");
        }

        var currentSubscription = await EnsureActiveSubscriptionAsync(applicationId, userId, cancellationToken);
        if (string.Equals(currentSubscription.PlanId, plan.Id, StringComparison.OrdinalIgnoreCase)
            && currentSubscription.Period == period)
        {
            return currentSubscription;
        }

        var now = DateTime.UtcNow;
        await _subscriptionStore.SaveAsync(currentSubscription with
        {
            Status = SubscriptionStatus.Expired,
            ExpiresAt = currentSubscription.ExpiresAt ?? now
        }, cancellationToken);

        var nextSubscription = new UserSubscription
        {
            ApplicationId = applicationId,
            UserId = userId,
            PlanId = plan.Id,
            Period = period,
            Status = SubscriptionStatus.Active,
            ActivatedAt = now,
            CreatedAt = now,
            ExpiresAt = CalculateExpiration(period, now),
            AutoRenew = autoRenew
        };

        await _subscriptionStore.SaveAsync(nextSubscription, cancellationToken);
        return nextSubscription;
    }

    public async Task<FeatureContext> BuildFeatureContextAsync(string applicationId, string userId, CancellationToken cancellationToken = default)
    {
        var subscription = await EnsureActiveSubscriptionAsync(applicationId, userId, cancellationToken);
        var plan = await _subscriptionCatalogService.GetPlanAsync(applicationId, subscription.PlanId, cancellationToken);

        return new FeatureContext
        {
            ApplicationId = applicationId,
            UserId = userId,
            SubscriptionPlanId = subscription.PlanId,
            SeedFeatures = plan.IncludedFeatures
        };
    }

    private static DateTime? CalculateExpiration(SubscriptionPeriod period, DateTime now) => period switch
    {
        SubscriptionPeriod.Monthly => now.AddMonths(1),
        SubscriptionPeriod.Annual => now.AddYears(1),
        SubscriptionPeriod.Permanent => null,
        _ => null
    };
}