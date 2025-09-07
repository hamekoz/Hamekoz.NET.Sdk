using System.Reflection;

using Hamekoz.Api.Models;
using Hamekoz.Api.Services;

using Microsoft.EntityFrameworkCore;

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

    public static void AddCrudServices<T>(
        this IServiceCollection services,
        Assembly? entitiesAssembly = null)
        where T : DbContext
    {
        entitiesAssembly ??= Assembly.GetCallingAssembly();

        // Obtener todas las clases derivadas de T
        var derivedTypes = entitiesAssembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && typeof(IEntity).IsAssignableFrom(t));

        foreach (var derivedType in derivedTypes)
        {
            // Crear tipos genéricos
            var serviceInterface = typeof(ICrudService<>).MakeGenericType(derivedType);
            var serviceClass = typeof(CrudService<,>).MakeGenericType(derivedType, typeof(T));

            services.AddScoped(serviceInterface, serviceClass);
        }
    }

    public static IServiceCollection AddUniqueImplementationOfServices(this IServiceCollection services, Assembly? assembly = null, string? namespaceKeyWordFilter = null)
    {
        assembly ??= Assembly.GetCallingAssembly();

        // Encontrar todas las interfaces"
        var interfaces = assembly.GetExportedTypes()
            .Where(t => t.FullName != null && t.FullName.Contains(namespaceKeyWordFilter ?? string.Empty) && t.IsInterface)
            .ToList();

        // Registrar solo las interfaces que tienen una única implementación
        foreach (var interfaceType in interfaces)
        {
            // Encontrar todas las implementaciones de interfaces"
            var implementations = assembly.GetExportedTypes()
                .Where(interfaceType.IsAssignableFrom)
                .Where(t => t.IsClass && !t.IsAbstract)
                .ToList();

            // Si hay exactamente una implementación, registrar en el contenedor
            if (implementations.Count == 1)
            {
                services.AddScoped(interfaceType, implementations[0]);
            }
        }

        return services;
    }

    public static void AddTemplateServices(
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
            services.AddScoped(serviceIntefaceType, serviceClassType);
        }
    }

    private static IEnumerable<Type> GetDerivedTypes(Type baseType, Assembly assembly)
    {
        // Iterar sobre los ensamblados y obtener los tipos que son subclases de la clase base
        foreach (var type in assembly!.GetTypes())
        {
            if (type.IsSubclassOf(baseType))
            {
                yield return type;
            }
        }
    }
}
