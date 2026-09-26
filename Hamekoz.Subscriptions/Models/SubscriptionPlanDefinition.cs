namespace Hamekoz.Subscriptions.Models;

public sealed record class SubscriptionPlanDefinition
{
    public required string ApplicationId { get; init; }
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public bool IsActive { get; init; } = true;
    public bool IsDefault { get; init; }
    public bool IsFree { get; init; }
    public IReadOnlyList<string> IncludedFeatures { get; init; } = [];
    public IReadOnlyList<SubscriptionPeriod> AvailablePeriods { get; init; } = [SubscriptionPeriod.Permanent];
}
