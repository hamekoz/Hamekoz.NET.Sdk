using Hamekoz.Features.Models;

namespace Hamekoz.Features.Services;

public sealed class FeatureService(IFeatureGrantStore featureGrantStore) : IFeatureService
{
    private readonly IFeatureGrantStore _featureGrantStore = featureGrantStore;

    public async Task<IReadOnlySet<string>> GetEffectiveFeaturesAsync(FeatureContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        var features = new HashSet<string>(context.SeedFeatures, StringComparer.OrdinalIgnoreCase);
        var grants = await _featureGrantStore.GetGrantsAsync(context.ApplicationId, cancellationToken);

        foreach (var grant in grants
                     .Where(grant => grant.AppliesTo(context))
                     .OrderBy(grant => grant.Scope))
        {
            if (grant.IsEnabled)
            {
                features.Add(grant.FeatureKey);
            }
            else
            {
                features.Remove(grant.FeatureKey);
            }
        }

        return features;
    }

    public async Task<bool> HasFeatureAsync(FeatureContext context, string featureKey, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(featureKey);

        var features = await GetEffectiveFeaturesAsync(context, cancellationToken);
        return features.Contains(featureKey);
    }
}