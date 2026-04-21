using Hamekoz.Features.Services;
using Hamekoz.Features.Stores;
using Microsoft.Extensions.DependencyInjection;

namespace Hamekoz.Features.Extensions;

public static class FeatureServiceCollectionExtensions
{
    public static IServiceCollection AddHamekozFeatures(this IServiceCollection services)
    {
        services.AddScoped<IFeatureService, FeatureService>();
        return services;
    }

    public static IServiceCollection AddInMemoryFeatureStore(this IServiceCollection services)
    {
        services.AddSingleton<IFeatureGrantStore, InMemoryFeatureGrantStore>();
        return services;
    }
}