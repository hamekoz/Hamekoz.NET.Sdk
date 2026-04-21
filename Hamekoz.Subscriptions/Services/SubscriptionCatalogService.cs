using Hamekoz.Api.Exceptions;
using Hamekoz.Subscriptions.Models;

namespace Hamekoz.Subscriptions.Services;

public sealed class SubscriptionCatalogService(ISubscriptionPlanStore planStore) : ISubscriptionCatalogService
{
    private const string DefaultFreePlanId = "free";
    private readonly ISubscriptionPlanStore _planStore = planStore;

    public Task<IReadOnlyList<SubscriptionPlanDefinition>> GetPlansAsync(string applicationId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(applicationId);
        return _planStore.GetPlansAsync(applicationId, cancellationToken);
    }

    public async Task<SubscriptionPlanDefinition> GetPlanAsync(string applicationId, string planId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(applicationId);
        ArgumentException.ThrowIfNullOrWhiteSpace(planId);

        var plans = await _planStore.GetPlansAsync(applicationId, cancellationToken);
        var plan = plans.FirstOrDefault(existing => string.Equals(existing.Id, planId, StringComparison.OrdinalIgnoreCase));

        return plan ?? throw new NotFoundException($"No se encontró el plan '{planId}' para la aplicación '{applicationId}'.");
    }

    public async Task<SubscriptionPlanDefinition> SavePlanAsync(SubscriptionPlanDefinition plan, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(plan);

        Validate(plan);

        var plans = await _planStore.GetPlansAsync(plan.ApplicationId, cancellationToken);

        if (plan.IsDefault)
        {
            foreach (var existing in plans.Where(existing => existing.IsDefault && !string.Equals(existing.Id, plan.Id, StringComparison.OrdinalIgnoreCase)))
            {
                await _planStore.SaveAsync(existing with { IsDefault = false }, cancellationToken);
            }
        }

        await _planStore.SaveAsync(plan, cancellationToken);
        await EnsureDefaultFreePlanAsync(plan.ApplicationId, cancellationToken);

        return plan;
    }

    public async Task<SubscriptionPlanDefinition> EnsureDefaultFreePlanAsync(string applicationId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(applicationId);

        var plans = await _planStore.GetPlansAsync(applicationId, cancellationToken);
        var activeDefault = plans.FirstOrDefault(plan => plan.IsDefault && plan.IsActive && plan.IsFree);
        if (activeDefault is not null)
        {
            return activeDefault;
        }

        var fallback = plans.FirstOrDefault(plan => string.Equals(plan.Id, DefaultFreePlanId, StringComparison.OrdinalIgnoreCase) && plan.IsFree);
        if (fallback is not null)
        {
            var normalized = fallback with { IsDefault = true, IsActive = true };
            await _planStore.SaveAsync(normalized, cancellationToken);
            return normalized;
        }

        var freePlan = new SubscriptionPlanDefinition
        {
            ApplicationId = applicationId,
            Id = DefaultFreePlanId,
            Name = "Free",
            Description = "Plan base gratuito del ecosistema Hamekoz.",
            IsActive = true,
            IsDefault = true,
            IsFree = true,
            IncludedFeatures = [],
            AvailablePeriods = [SubscriptionPeriod.Permanent]
        };

        await _planStore.SaveAsync(freePlan, cancellationToken);
        return freePlan;
    }

    private static void Validate(SubscriptionPlanDefinition plan)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(plan.ApplicationId);
        ArgumentException.ThrowIfNullOrWhiteSpace(plan.Id);
        ArgumentException.ThrowIfNullOrWhiteSpace(plan.Name);
        ArgumentException.ThrowIfNullOrWhiteSpace(plan.Description);

        if (plan.IsDefault && !plan.IsFree)
        {
            throw new ValidationException("El plan por defecto debe ser gratuito.");
        }

        if (!plan.IsActive && plan.IsDefault)
        {
            throw new ValidationException("El plan por defecto debe estar activo.");
        }

        if (plan.AvailablePeriods.Count == 0)
        {
            throw new ValidationException("El plan debe exponer al menos un período disponible.");
        }
    }
}
