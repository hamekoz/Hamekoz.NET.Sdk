using Hamekoz.Api.Exceptions;
using Hamekoz.Subscriptions.Models;
using Hamekoz.Subscriptions.Services;
using Hamekoz.Subscriptions.Stores;

namespace Hamekoz.Api.Tests.Subscriptions;

public class SubscriptionServiceTests
{
    private readonly InMemorySubscriptionPlanStore _planStore = new();
    private readonly InMemorySubscriptionStore _subscriptionStore = new();

    [Fact]
    public async Task EnsureActiveSubscriptionAsync_Should_CreateDefaultFreePlanAndSubscription()
    {
        var catalogService = new SubscriptionCatalogService(_planStore);
        var service = new SubscriptionService(catalogService, _planStore, _subscriptionStore);

        var subscription = await service.EnsureActiveSubscriptionAsync("mi-agenda-docente", "user-1");
        var plans = await catalogService.GetPlansAsync("mi-agenda-docente");

        Assert.Equal("free", subscription.PlanId);
        Assert.Equal(SubscriptionPeriod.Permanent, subscription.Period);
        Assert.True(subscription.IsActive);
        Assert.Single(plans);
        Assert.True(plans[0].IsDefault);
        Assert.True(plans[0].IsFree);
        Assert.True(plans[0].IsActive);
    }

    [Fact]
    public async Task AssignPlanAsync_Should_ReplaceCurrentSubscriptionAndExposePlanFeatures()
    {
        var catalogService = new SubscriptionCatalogService(_planStore);
        var service = new SubscriptionService(catalogService, _planStore, _subscriptionStore);

        await catalogService.SavePlanAsync(new SubscriptionPlanDefinition
        {
            ApplicationId = "carta-universal",
            Id = "pro",
            Name = "Pro",
            Description = "Plan profesional",
            IsActive = true,
            IsDefault = false,
            IsFree = false,
            IncludedFeatures = ["billing.export", "courses.unlimited"],
            AvailablePeriods = [SubscriptionPeriod.Monthly, SubscriptionPeriod.Annual]
        });

        var assigned = await service.AssignPlanAsync("carta-universal", "user-2", "pro", SubscriptionPeriod.Monthly, autoRenew: true);
        var subscriptions = await _subscriptionStore.GetUserSubscriptionsAsync("carta-universal", "user-2");
        var featureContext = await service.BuildFeatureContextAsync("carta-universal", "user-2");

        Assert.Equal("pro", assigned.PlanId);
        Assert.Equal(SubscriptionPeriod.Monthly, assigned.Period);
        Assert.True(assigned.AutoRenew);
        Assert.Equal(2, subscriptions.Count);
        Assert.Contains(subscriptions, subscription => subscription.PlanId == "free" && subscription.Status == SubscriptionStatus.Expired && !subscription.AutoRenew && subscription.ExpiresAt is not null);
        Assert.Equal(["billing.export", "courses.unlimited"], featureContext.SeedFeatures);
    }

    [Fact]
    public async Task EnsureActiveSubscriptionAsync_Should_UseFreePlanAvailablePeriod_WhenPermanentIsNotAvailable()
    {
        var catalogService = new SubscriptionCatalogService(_planStore);
        var service = new SubscriptionService(catalogService, _planStore, _subscriptionStore);
        await catalogService.SavePlanAsync(new SubscriptionPlanDefinition
        {
            ApplicationId = "app-1",
            Id = "free",
            Name = "Free",
            Description = "Plan gratuito",
            IsActive = true,
            IsDefault = true,
            IsFree = true,
            IncludedFeatures = [],
            AvailablePeriods = [SubscriptionPeriod.Monthly]
        });

        var subscription = await service.EnsureActiveSubscriptionAsync("app-1", "user-1");

        Assert.Equal(SubscriptionPeriod.Monthly, subscription.Period);
        Assert.NotNull(subscription.ExpiresAt);
    }

    [Fact]
    public async Task AssignPlanAsync_Should_ThrowNotFoundException_WhenPlanDoesNotExist()
    {
        var catalogService = new SubscriptionCatalogService(_planStore);
        var service = new SubscriptionService(catalogService, _planStore, _subscriptionStore);

        var action = () => service.AssignPlanAsync("app-1", "user-1", "missing-plan", SubscriptionPeriod.Monthly);

        await Assert.ThrowsAsync<NotFoundException>(action);
    }

    [Fact]
    public async Task EnsureActiveSubscriptionAsync_Should_ThrowValidationException_WhenFreePlanHasNoSupportedPeriod()
    {
        var catalogService = new SubscriptionCatalogService(_planStore);
        var service = new SubscriptionService(catalogService, _planStore, _subscriptionStore);
        await catalogService.SavePlanAsync(new SubscriptionPlanDefinition
        {
            ApplicationId = "app-2",
            Id = "free",
            Name = "Free",
            Description = "Plan gratuito",
            IsActive = true,
            IsDefault = true,
            IsFree = true,
            IncludedFeatures = [],
            AvailablePeriods = [(SubscriptionPeriod)99]
        });

        var action = () => service.EnsureActiveSubscriptionAsync("app-2", "user-1");

        await Assert.ThrowsAsync<ValidationException>(action);
    }
}
