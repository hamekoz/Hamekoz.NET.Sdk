namespace Hamekoz.Api.Services;

/// <summary>
/// Provides feature flag management with three hierarchical levels.
/// </summary>
public interface IFeatureService
{
    /// <summary>
    /// Sets a feature state at level 1 (global).
    /// </summary>
    public Task SetLevel1Async(string featureKey, bool isEnabled, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets a feature state at level 2.
    /// </summary>
    public Task SetLevel2Async(string featureKey, string level2Key, bool isEnabled, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets a feature state at level 3.
    /// </summary>
    public Task SetLevel3Async(string featureKey, string level2Key, string level3Key, bool isEnabled, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resolves if a feature is enabled using precedence: level 3 > level 2 > level 1 > defaultValue.
    /// </summary>
    public Task<bool> IsEnabledAsync(
        string featureKey,
        string? level2Key = null,
        string? level3Key = null,
        bool defaultValue = false,
        CancellationToken cancellationToken = default);
}
