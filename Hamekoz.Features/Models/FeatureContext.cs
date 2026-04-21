namespace Hamekoz.Features.Models;

public sealed record class FeatureContext
{
    public required string ApplicationId { get; init; }
    public string? SubscriptionPlanId { get; init; }
    public string? UserId { get; init; }
    public IReadOnlyCollection<string> SeedFeatures { get; init; } = [];
}