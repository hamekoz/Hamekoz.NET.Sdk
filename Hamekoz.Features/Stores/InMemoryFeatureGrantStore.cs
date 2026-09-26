using Hamekoz.Features.Models;
using Hamekoz.Features.Services;

namespace Hamekoz.Features.Stores;

public sealed class InMemoryFeatureGrantStore : IFeatureGrantStore
{
    private readonly Lock _lock = new();
    private readonly List<FeatureGrant> _grants = [];

    public Task<IReadOnlyList<FeatureGrant>> GetGrantsAsync(string applicationId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(applicationId);

        lock (_lock)
        {
            var grants = _grants
                .Where(grant => string.Equals(grant.ApplicationId, applicationId, StringComparison.OrdinalIgnoreCase))
                .ToArray();

            return Task.FromResult<IReadOnlyList<FeatureGrant>>(grants);
        }
    }

    public Task SaveAsync(FeatureGrant grant, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(grant);

        lock (_lock)
        {
            var index = _grants.FindIndex(existing => string.Equals(existing.Id, grant.Id, StringComparison.OrdinalIgnoreCase));
            if (index >= 0)
            {
                _grants[index] = grant;
            }
            else
            {
                _grants.Add(grant);
            }
        }

        return Task.CompletedTask;
    }
}
