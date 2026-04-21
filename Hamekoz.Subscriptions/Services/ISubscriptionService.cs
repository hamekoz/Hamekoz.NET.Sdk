using Hamekoz.Features.Models;
using Hamekoz.Subscriptions.Models;

namespace Hamekoz.Subscriptions.Services;

public interface ISubscriptionService
{
    Task<UserSubscription> EnsureActiveSubscriptionAsync(string applicationId, string userId, CancellationToken cancellationToken = default);

    Task<UserSubscription> AssignPlanAsync(
        string applicationId,
        string userId,
        string planId,
        SubscriptionPeriod period,
        bool autoRenew = false,
        CancellationToken cancellationToken = default);

    Task<FeatureContext> BuildFeatureContextAsync(string applicationId, string userId, CancellationToken cancellationToken = default);
}