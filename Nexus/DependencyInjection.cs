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
            .Where(t => !t.IsAbstract && !t.IsInterface);

        foreach (var type in types)
        {
            var interfaces = type.GetInterfaces();

            var requestHandlers = interfaces
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == requestHandlerType);
            foreach (var handlerInterface in requestHandlers)
            {
                services.AddTransient(handlerInterface, type);
            }

            var notificationHandlers = interfaces
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == notificationHandlerType);
            foreach (var handlerInterface in notificationHandlers)
            {
                services.AddTransient(handlerInterface, type);
            }
        }
        
        return services;
    }
}