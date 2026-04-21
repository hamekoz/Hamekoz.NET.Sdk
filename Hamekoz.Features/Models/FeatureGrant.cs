namespace Hamekoz.Features.Models;

public sealed record class FeatureGrant
{
    public string Id { get; init; } = Guid.NewGuid().ToString("N");
    public required string ApplicationId { get; init; }
    public required string FeatureKey { get; init; }
    public FeatureScope Scope { get; init; } = FeatureScope.Application;
    public string? ScopeId { get; init; }
    public bool IsEnabled { get; init; } = true;

    public bool AppliesTo(FeatureContext context)
    {
        if (!string.Equals(ApplicationId, context.ApplicationId, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return Scope switch
        {
            FeatureScope.Application => true,
            FeatureScope.Subscription => !string.IsNullOrWhiteSpace(context.SubscriptionPlanId)
                && string.Equals(ScopeId, context.SubscriptionPlanId, StringComparison.OrdinalIgnoreCase),
            FeatureScope.User => !string.IsNullOrWhiteSpace(context.UserId)
                && string.Equals(ScopeId, context.UserId, StringComparison.OrdinalIgnoreCase),
            _ => false
        };
    }
}