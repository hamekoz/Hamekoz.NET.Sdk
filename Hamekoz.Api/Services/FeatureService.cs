using System.Collections.Concurrent;

using Hamekoz.Api.Exceptions;

namespace Hamekoz.Api.Services;

/// <summary>
/// In-memory implementation of feature management with three hierarchical levels.
/// </summary>
public class FeatureService : IFeatureService
{
    private readonly ConcurrentDictionary<string, bool> _level1 = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, bool>> _level2 = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, ConcurrentDictionary<string, bool>>> _level3 = new(StringComparer.OrdinalIgnoreCase);

    public Task SetLevel1Async(string featureKey, bool isEnabled, CancellationToken cancellationToken = default)
    {
        var normalizedFeatureKey = NormalizeRequired(featureKey, nameof(featureKey));
        cancellationToken.ThrowIfCancellationRequested();
        _level1[normalizedFeatureKey] = isEnabled;
        return Task.CompletedTask;
    }

    public Task SetLevel2Async(string featureKey, string level2Key, bool isEnabled, CancellationToken cancellationToken = default)
    {
        var normalizedFeatureKey = NormalizeRequired(featureKey, nameof(featureKey));
        var normalizedLevel2Key = NormalizeRequired(level2Key, nameof(level2Key));

        cancellationToken.ThrowIfCancellationRequested();

        var featureLevel2 = _level2.GetOrAdd(
            normalizedFeatureKey,
            _ => new ConcurrentDictionary<string, bool>(StringComparer.OrdinalIgnoreCase));

        featureLevel2[normalizedLevel2Key] = isEnabled;
        return Task.CompletedTask;
    }

    public Task SetLevel3Async(string featureKey, string level2Key, string level3Key, bool isEnabled, CancellationToken cancellationToken = default)
    {
        var normalizedFeatureKey = NormalizeRequired(featureKey, nameof(featureKey));
        var normalizedLevel2Key = NormalizeRequired(level2Key, nameof(level2Key));
        var normalizedLevel3Key = NormalizeRequired(level3Key, nameof(level3Key));

        cancellationToken.ThrowIfCancellationRequested();

        var featureLevel3 = _level3.GetOrAdd(
            normalizedFeatureKey,
            _ => new ConcurrentDictionary<string, ConcurrentDictionary<string, bool>>(StringComparer.OrdinalIgnoreCase));

        var featureLevel2 = featureLevel3.GetOrAdd(
            normalizedLevel2Key,
            _ => new ConcurrentDictionary<string, bool>(StringComparer.OrdinalIgnoreCase));

        featureLevel2[normalizedLevel3Key] = isEnabled;
        return Task.CompletedTask;
    }

    public Task<bool> IsEnabledAsync(
        string featureKey,
        string? level2Key = null,
        string? level3Key = null,
        bool defaultValue = false,
        CancellationToken cancellationToken = default)
    {
        var normalizedFeatureKey = NormalizeRequired(featureKey, nameof(featureKey));
        var normalizedLevel2Key = NormalizeOptional(level2Key);
        var normalizedLevel3Key = NormalizeOptional(level3Key);

        cancellationToken.ThrowIfCancellationRequested();

        if (normalizedLevel3Key is not null && normalizedLevel2Key is null)
        {
            throw new ValidationException("level2Key is required when level3Key is provided.");
        }

        if (normalizedLevel2Key is not null && normalizedLevel3Key is not null)
        {
            if (_level3.TryGetValue(normalizedFeatureKey, out var featureLevel3)
                && featureLevel3.TryGetValue(normalizedLevel2Key, out var level3ByLevel2)
                && level3ByLevel2.TryGetValue(normalizedLevel3Key, out var level3Value))
            {
                return Task.FromResult(level3Value);
            }
        }

        if (normalizedLevel2Key is not null)
        {
            if (_level2.TryGetValue(normalizedFeatureKey, out var featureLevel2) && featureLevel2.TryGetValue(normalizedLevel2Key, out var level2Value))
            {
                return Task.FromResult(level2Value);
            }
        }

        if (_level1.TryGetValue(normalizedFeatureKey, out var level1Value))
        {
            return Task.FromResult(level1Value);
        }

        return Task.FromResult(defaultValue);
    }

    private static string NormalizeRequired(string value, string parameterName)
    {
        var normalized = NormalizeOptional(value);
        if (normalized is null)
        {
            throw new ValidationException($"{parameterName} is required.");
        }

        return normalized;
    }

    private static string? NormalizeOptional(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim();
    }
}
