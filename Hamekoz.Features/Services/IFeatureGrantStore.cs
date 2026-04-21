using Hamekoz.Features.Models;

namespace Hamekoz.Features.Services;

public interface IFeatureGrantStore
{
    Task<IReadOnlyList<FeatureGrant>> GetGrantsAsync(string applicationId, CancellationToken cancellationToken = default);

    Task SaveAsync(FeatureGrant grant, CancellationToken cancellationToken = default);
}