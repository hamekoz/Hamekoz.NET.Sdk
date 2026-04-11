using System.Reflection;

using Hamekoz.Api.Models;
using Hamekoz.Api.Services;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Hamekoz.Api.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add Hamekoz Api Services
    /// </summary>
    /// <typeparam name="TDbContext">Database Context Type</typeparam>
    /// <param name="services"></param>
    /// <param name="entitiesAssembly">Assembly where are declared entities derived from <cref>ICrudEntity</cref></param>
    /// <returns>IServiceCollection</returns>
    public static IServiceCollection AddHamekozApi<TDbContext>(
        this IServiceCollection services,
        Assembly? entitiesAssembly = null)
        where TDbContext : DbContext
    {
        entitiesAssembly ??= Assembly.GetCallingAssembly();

        services.AddCrudServices<TDbContext>(entitiesAssembly);

        return services;
    }

    public static IServiceCollection AddCrudServices<T>(
        this IServiceCollection services,
        Assembly? entitiesAssembly = null)
        where T : DbContext
    {
        entitiesAssembly ??= Assembly.GetCallingAssembly();

        var derivedTypes = GetLoadableTypes(entitiesAssembly)
            .Where(t => t.IsClass && !t.IsAbstract && !t.ContainsGenericParameters && typeof(Entity).IsAssignableFrom(t))
            .OrderBy(t => t.FullName);

        foreach (var derivedType in derivedTypes)
        {
            var serviceInterface = typeof(ICrudService<>).MakeGenericType(derivedType);
            var serviceClass = typeof(CrudService<,>).MakeGenericType(derivedType, typeof(T));

            services.TryAddScoped(serviceInterface, serviceClass);
        }

        return services;
    }

    public static IServiceCollection AddUniqueImplementationOfServices(this IServiceCollection services, Assembly? assembly = null, string? namespaceKeyWordFilter = null)
    {
        assembly ??= Assembly.GetCallingAssembly();
        var loadableTypes = GetLoadableTypes(assembly)
            .Where(t => t.IsPublic)
            .ToList();

        var interfaces = loadableTypes
            .Where(t => t.IsInterface && !t.IsGenericTypeDefinition && t.FullName != null && t.FullName.Contains(namespaceKeyWordFilter ?? string.Empty))
            .ToList();

        foreach (var interfaceType in interfaces)
        {
            var implementations = loadableTypes
                .Where(interfaceType.IsAssignableFrom)
                .Where(t => t.IsClass && !t.IsAbstract && !t.ContainsGenericParameters)
                .ToList();

            if (implementations.Count == 1)
            {
                services.TryAddScoped(interfaceType, implementations[0]);
            }
        }

        return services;
    }

    public static IServiceCollection AddTemplateServices(
        this IServiceCollection services,
        Type baseType,
        Type serviceTemplateInterfaceType,
        Type serviceTemplateClassType,
        Assembly? assembly = null)
    {
        var resolvedAssembly = (assembly ?? Assembly.GetAssembly(baseType)) ?? Assembly.GetCallingAssembly();

        var types = GetDerivedTypes(baseType, resolvedAssembly);

        foreach (var type in types)
        {
            var serviceIntefaceType = serviceTemplateInterfaceType.MakeGenericType(type);
            var serviceClassType = serviceTemplateClassType.MakeGenericType(type);
            services.TryAddScoped(serviceIntefaceType, serviceClassType);
        }

        return services;
    }

    private static IEnumerable<Type> GetDerivedTypes(Type baseType, Assembly assembly)
    {
        return GetLoadableTypes(assembly)
            .Where(type => type.IsClass && !type.IsAbstract && !type.ContainsGenericParameters && type.IsSubclassOf(baseType))
            .OrderBy(type => type.FullName);
    }

    private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            return ex.Types.Where(t => t is not null)!;
        }
    }
}
