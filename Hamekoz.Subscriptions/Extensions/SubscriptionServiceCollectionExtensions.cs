using Hamekoz.Subscriptions.Services;
using Hamekoz.Subscriptions.Stores;

using Microsoft.Extensions.DependencyInjection;

namespace Hamekoz.Subscriptions.Extensions;

public static class SubscriptionServiceCollectionExtensions
{
    public static IServiceCollection AddHamekozSubscriptions(this IServiceCollection services)
    {
        services.AddScoped<ISubscriptionCatalogService, SubscriptionCatalogService>();
        services.AddScoped<ISubscriptionService, SubscriptionService>();
        return services;
    }

    public static IServiceCollection AddInMemorySubscriptionStores(this IServiceCollection services)
    {
        services.AddSingleton<ISubscriptionPlanStore, InMemorySubscriptionPlanStore>();
        services.AddSingleton<ISubscriptionStore, InMemorySubscriptionStore>();
        return services;
    }
}
