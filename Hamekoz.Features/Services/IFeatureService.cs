using Hamekoz.Features.Models;

namespace Hamekoz.Features.Services;

public interface IFeatureService
{
    Task<IReadOnlySet<string>> GetEffectiveFeaturesAsync(FeatureContext context, CancellationToken cancellationToken = default);

    Task<bool> HasFeatureAsync(FeatureContext context, string featureKey, CancellationToken cancellationToken = default);
}