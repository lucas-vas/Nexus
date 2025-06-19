// Nexus/DependencyInjection.cs
using Nexus.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Nexus;

/// <summary>
/// Extension methods for registering Nexus services in the dependency injection container.
/// </summary>
public static class DependencyInjection
{
    private static readonly Type RequestHandlerType = typeof(IRequestHandler<,>);
    private static readonly Type RequestHandlerWithoutResponseType = typeof(IRequestHandler<>);
    private static readonly Type NotificationHandlerType = typeof(INotificationHandler<>);

    /// <summary>
    /// Registers Nexus services and automatically discovers handlers in the specified assembly.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="assembly">The assembly to scan for handlers.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when services or assembly is null.</exception>
    public static IServiceCollection AddNexus(this IServiceCollection services, Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(services, nameof(services));
        ArgumentNullException.ThrowIfNull(assembly, nameof(assembly));

        // Register the main Nexus implementation
        services.AddScoped<INexus, Nexus>();

        var types = assembly.GetTypes()
            .Where(t => !t.IsAbstract && !t.IsInterface && !t.IsGenericType && t.IsPublic);

        foreach (var type in types)
        {
            try
            {
                RegisterHandlerType(services, type);
            }
            catch (Exception ex)
            {
                // Log the error but don't fail the registration of other handlers
                Console.WriteLine($"Warning: Failed to register handler type '{type.Name}': {ex.Message}");
            }
        }
        
        return services;
    }

    /// <summary>
    /// Registers Nexus services and automatically discovers handlers in multiple assemblies.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="assemblies">The assemblies to scan for handlers.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when services or assemblies is null.</exception>
    public static IServiceCollection AddNexus(this IServiceCollection services, params Assembly[] assemblies)
    {
        ArgumentNullException.ThrowIfNull(services, nameof(services));
        ArgumentNullException.ThrowIfNull(assemblies, nameof(assemblies));

        foreach (var assembly in assemblies)
        {
            if (assembly != null)
            {
                services.AddNexus(assembly);
            }
        }
        return services;
    }

    /// <summary>
    /// Debug method to list all registered handlers in the service provider.
    /// </summary>
    /// <param name="serviceProvider">The service provider to inspect.</param>
    /// <exception cref="ArgumentNullException">Thrown when serviceProvider is null.</exception>
    public static void DebugRegisteredHandlers(this IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider, nameof(serviceProvider));

        Console.WriteLine("=== Registered Handlers ===");
        
        try
        {
            var services = serviceProvider.GetServices<object>().ToList();
            
            DebugHandlerType(services, RequestHandlerType, "Request Handler (with response)");
            DebugHandlerType(services, RequestHandlerWithoutResponseType, "Request Handler (without response)");
            DebugHandlerType(services, NotificationHandlerType, "Notification Handler");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error listing handlers: {ex.Message}");
        }
    }

    /// <summary>
    /// Registers a handler type in the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="type">The type to register.</param>
    private static void RegisterHandlerType(IServiceCollection services, Type type)
    {
        var interfaces = type.GetInterfaces();

        // Register Request Handlers with response
        RegisterHandlersByType(services, type, interfaces, RequestHandlerType);

        // Register Request Handlers without response
        RegisterHandlersByType(services, type, interfaces, RequestHandlerWithoutResponseType);

        // Register Notification Handlers
        RegisterHandlersByType(services, type, interfaces, NotificationHandlerType);
    }

    /// <summary>
    /// Registers handlers of a specific type in the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="type">The implementation type.</param>
    /// <param name="interfaces">The interfaces implemented by the type.</param>
    /// <param name="handlerType">The handler type to register.</param>
    private static void RegisterHandlersByType(IServiceCollection services, Type type, Type[] interfaces, Type handlerType)
    {
        var handlers = interfaces
            .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == handlerType);
        
        foreach (var handlerInterface in handlers)
        {
            services.AddTransient(handlerInterface, type);
        }
    }

    /// <summary>
    /// Debug method to list handlers of a specific type.
    /// </summary>
    /// <param name="services">The list of services.</param>
    /// <param name="handlerType">The handler type to look for.</param>
    /// <param name="label">The label to display.</param>
    private static void DebugHandlerType(List<object> services, Type handlerType, string label)
    {
        var handlers = services
            .Where(s => s?.GetType().GetInterfaces()
                .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == handlerType) == true)
            .Select(s => s.GetType())
            .Distinct();

        foreach (var handler in handlers)
        {
            Console.WriteLine($"{label}: {handler.FullName}");
        }
    }
}