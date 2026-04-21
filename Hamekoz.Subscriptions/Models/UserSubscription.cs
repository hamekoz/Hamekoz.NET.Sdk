namespace Hamekoz.Subscriptions.Models;

public sealed record class UserSubscription
{
    public string Id { get; init; } = Guid.NewGuid().ToString("N");
    public required string ApplicationId { get; init; }
    public required string UserId { get; init; }
    public required string PlanId { get; init; }
    public SubscriptionPeriod Period { get; init; } = SubscriptionPeriod.Permanent;
    public SubscriptionStatus Status { get; init; } = SubscriptionStatus.Active;
    public bool AutoRenew { get; init; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime? ActivatedAt { get; init; } = DateTime.UtcNow;
    public DateTime? ExpiresAt { get; init; }

    public bool IsActive => Status == SubscriptionStatus.Active && (ExpiresAt is null || ExpiresAt > DateTime.UtcNow);
}