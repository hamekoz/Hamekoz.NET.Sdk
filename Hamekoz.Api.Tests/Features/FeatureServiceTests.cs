using Hamekoz.Features.Models;
using Hamekoz.Features.Services;
using Hamekoz.Features.Stores;

namespace Hamekoz.Api.Tests.Features;

public class FeatureServiceTests
{
    private readonly IFeatureGrantStore _grantStore = new InMemoryFeatureGrantStore();

    [Fact]
    public async Task GetEffectiveFeaturesAsync_Should_MergeApplicationPlanAndUserGrants()
    {
        await _grantStore.SaveAsync(new FeatureGrant
        {
            ApplicationId = "carta-universal",
            FeatureKey = "shared.analytics",
            Scope = FeatureScope.Application,
            IsEnabled = true
        });

        await _grantStore.SaveAsync(new FeatureGrant
        {
            ApplicationId = "carta-universal",
            FeatureKey = "billing.export",
            Scope = FeatureScope.Subscription,
            ScopeId = "pro",
            IsEnabled = true
        });

        await _grantStore.SaveAsync(new FeatureGrant
        {
            ApplicationId = "carta-universal",
            FeatureKey = "billing.export",
            Scope = FeatureScope.User,
            ScopeId = "user-1",
            IsEnabled = false
        });

        await _grantStore.SaveAsync(new FeatureGrant
        {
            ApplicationId = "carta-universal",
            FeatureKey = "beta.teacher-ai",
            Scope = FeatureScope.User,
            ScopeId = "user-1",
            IsEnabled = true
        });

        var service = new FeatureService(_grantStore);

        var result = await service.GetEffectiveFeaturesAsync(new FeatureContext
        {
            ApplicationId = "carta-universal",
            UserId = "user-1",
            SubscriptionPlanId = "pro",
            SeedFeatures = ["content.read"]
        });

        Assert.Contains("content.read", result);
        Assert.Contains("shared.analytics", result);
        Assert.Contains("beta.teacher-ai", result);
        Assert.DoesNotContain("billing.export", result);
    }
}