using Microsoft.Extensions.DependencyInjection;
using Nexus.Interfaces;

namespace Nexus.Tests.Common;

public static class TestServiceProviderHelpers
{
    public static ServiceProvider BuildProviderWithHandlersAndBehaviors<TRequest, TResponse>(
        IRequestHandler<TRequest, TResponse> handler,
        params IPipelineBehavior<TRequest, TResponse>[] behaviors)
        where TRequest : IRequest<TResponse>
    {
        var services = new ServiceCollection();
        services.AddScoped<IRequestHandler<TRequest, TResponse>>(_ => handler);
        foreach (var behavior in behaviors)
        {
            services.AddScoped<IPipelineBehavior<TRequest, TResponse>>(_ => behavior);
        }
        return services.BuildServiceProvider();
    }

    public static ServiceProvider BuildProviderWithNotificationHandlers<TNotification>(
        params INotificationHandler<TNotification>[] handlers)
        where TNotification : INotification
    {
        var services = new ServiceCollection();
        foreach (var handler in handlers)
        {
            services.AddScoped<INotificationHandler<TNotification>>(_ => handler);
        }
        return services.BuildServiceProvider();
    }
} 