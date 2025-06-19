// Nexus/DependencyInjection.cs
using Nexus.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Nexus;

public static class DependencyInjection
{
    public static IServiceCollection AddNexus(this IServiceCollection services, Assembly assembly)
    {
        // Registra a implementação principal do Nexus
        services.AddScoped<INexus, Nexus>();

        var requestHandlerType = typeof(IRequestHandler<,>);
        var notificationHandlerType = typeof(INotificationHandler<>);

        var types = assembly.GetTypes()
            .Where(t => !t.IsAbstract && !t.IsInterface && !t.IsGenericType);

        foreach (var type in types)
        {
            var interfaces = type.GetInterfaces();

            // Registra Request Handlers
            var requestHandlers = interfaces
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == requestHandlerType);
            foreach (var handlerInterface in requestHandlers)
            {
                services.AddTransient(handlerInterface, type);
            }

            // Registra Notification Handlers
            var notificationHandlers = interfaces
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == notificationHandlerType);
            foreach (var handlerInterface in notificationHandlers)
            {
                services.AddTransient(handlerInterface, type);
            }
        }
        
        return services;
    }

    public static IServiceCollection AddNexus(this IServiceCollection services, params Assembly[] assemblies)
    {
        foreach (var assembly in assemblies)
        {
            services.AddNexus(assembly);
        }
        return services;
    }

    public static void DebugRegisteredHandlers(this IServiceProvider serviceProvider)
    {
        var requestHandlerType = typeof(IRequestHandler<,>);
        var notificationHandlerType = typeof(INotificationHandler<>);

        Console.WriteLine("=== Handlers Registrados ===");
        
        // Lista todos os tipos registrados que implementam IRequestHandler
        var requestHandlers = serviceProvider.GetServices<object>()
            .Where(s => s.GetType().GetInterfaces()
                .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == requestHandlerType))
            .Select(s => s.GetType());

        foreach (var handler in requestHandlers)
        {
            Console.WriteLine($"Request Handler: {handler.FullName}");
        }

        // Lista todos os tipos registrados que implementam INotificationHandler
        var notificationHandlers = serviceProvider.GetServices<object>()
            .Where(s => s.GetType().GetInterfaces()
                .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == notificationHandlerType))
            .Select(s => s.GetType());

        foreach (var handler in notificationHandlers)
        {
            Console.WriteLine($"Notification Handler: {handler.FullName}");
        }
    }
}