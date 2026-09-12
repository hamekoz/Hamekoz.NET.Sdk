using System.Collections.Concurrent;

using Hamekoz.Api.Exceptions;
using Hamekoz.Features.Models;
using Hamekoz.Subscriptions.Models;

namespace Hamekoz.Subscriptions.Services;

public sealed class SubscriptionService(
    ISubscriptionCatalogService subscriptionCatalogService,
    ISubscriptionPlanStore planStore,
    ISubscriptionStore subscriptionStore) : ISubscriptionService, IDisposable
{
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _activeSubscriptionLocks = new(StringComparer.OrdinalIgnoreCase);
    private readonly ISubscriptionCatalogService _subscriptionCatalogService = subscriptionCatalogService;
    private readonly ISubscriptionPlanStore _planStore = planStore;
    private readonly ISubscriptionStore _subscriptionStore = subscriptionStore;
    private bool _disposed;

    public void Dispose()
    {
        _disposed = true;
        foreach (var semaphore in _activeSubscriptionLocks.Values)
        {
            semaphore.Dispose();
        }

        _activeSubscriptionLocks.Clear();
    }

    public async Task<UserSubscription> EnsureActiveSubscriptionAsync(string applicationId, string userId, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        ArgumentException.ThrowIfNullOrWhiteSpace(applicationId);
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        var key = $"{applicationId}:{userId}";
        var keyLock = _activeSubscriptionLocks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
        await keyLock.WaitAsync(cancellationToken);
        try
        {
            var activeSubscription = await _subscriptionStore.GetActiveSubscriptionAsync(applicationId, userId, cancellationToken);
            if (activeSubscription is not null)
            {
                return activeSubscription;
            }

            var freePlan = await _subscriptionCatalogService.EnsureDefaultFreePlanAsync(applicationId, cancellationToken);
            var period = ResolveDefaultFreePeriod(freePlan);
            var now = DateTime.UtcNow;

            var subscription = new UserSubscription
            {
                ApplicationId = applicationId,
                UserId = userId,
                PlanId = freePlan.Id,
                Period = period,
                Status = SubscriptionStatus.Active,
                ActivatedAt = now,
                ExpiresAt = CalculateExpiration(period, now),
                AutoRenew = false
            };

            await _subscriptionStore.SaveAsync(subscription, cancellationToken);
            return subscription;
        }
        finally
        {
            keyLock.Release();
        }
    }

    public async Task<UserSubscription> AssignPlanAsync(
        string applicationId,
        string userId,
        string planId,
        SubscriptionPeriod period,
        bool autoRenew = false,
        CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        ArgumentException.ThrowIfNullOrWhiteSpace(applicationId);
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(planId);

        await _subscriptionCatalogService.EnsureDefaultFreePlanAsync(applicationId, cancellationToken);

        var plans = await _planStore.GetPlansAsync(applicationId, cancellationToken);
        var plan = plans.FirstOrDefault(existing => string.Equals(existing.Id, planId, StringComparison.OrdinalIgnoreCase))
            ?? throw new NotFoundException($"No se encontró el plan '{planId}' para la aplicación '{applicationId}'.");

        if (!plan.IsActive)
        {
            throw new ValidationException($"El plan '{planId}' no se encuentra activo.");
        }

        if (!plan.AvailablePeriods.Contains(period))
        {
            throw new ValidationException($"El plan '{planId}' no soporta el período '{period}'.");
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
            ExpiresAt = now,
            AutoRenew = false
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
        ThrowIfDisposed();
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
        SubscriptionPeriod.Weekly => now.AddDays(7),
        SubscriptionPeriod.Monthly => now.AddMonths(1),
        SubscriptionPeriod.Annual => now.AddYears(1),
        SubscriptionPeriod.Permanent => null,
        _ => throw new ArgumentOutOfRangeException(nameof(period), period, "Unsupported subscription period.")
    };

    private static SubscriptionPeriod ResolveDefaultFreePeriod(SubscriptionPlanDefinition freePlan)
    {
        if (freePlan.AvailablePeriods.Contains(SubscriptionPeriod.Permanent))
        {
            return SubscriptionPeriod.Permanent;
        }

        if (freePlan.AvailablePeriods.Contains(SubscriptionPeriod.Annual))
        {
            return SubscriptionPeriod.Annual;
        }

        if (freePlan.AvailablePeriods.Contains(SubscriptionPeriod.Monthly))
        {
            return SubscriptionPeriod.Monthly;
        }

        if (freePlan.AvailablePeriods.Contains(SubscriptionPeriod.Weekly))
        {
            return SubscriptionPeriod.Weekly;
        }

        throw new ValidationException($"El plan '{freePlan.Id}' no expone períodos soportados para asignación automática.");
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }
}
