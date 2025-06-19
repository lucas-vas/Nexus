using Nexus.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Nexus;

public sealed class Nexus(IServiceProvider serviceProvider) : INexus
{
    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        var requestType = request.GetType();
        var handlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, typeof(TResponse));

        var handler = serviceProvider.GetRequiredService(handlerType);

        var behaviors = serviceProvider.GetServices<IPipelineBehavior<IRequest<TResponse>, TResponse>>().Reverse();

        var handlerDelegate = new RequestHandlerDelegate<TResponse>(() => (Task<TResponse>)handlerType
            .GetMethod("Handle")!
            .Invoke(handler, [request, cancellationToken])!);

        var pipeline = behaviors.Aggregate(
            (RequestHandlerDelegate<TResponse>)handlerDelegate,
            (next, pipeline) => () => pipeline.Handle((IRequest<TResponse>)request, next, cancellationToken)
        );

        return pipeline();
    }

    public async Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
        where TNotification : INotification
    {
        var notificationHandlers = serviceProvider.GetServices<INotificationHandler<TNotification>>();

        var tasks = notificationHandlers.Select(handler => handler.Handle(notification, cancellationToken));
        
        await Task.WhenAll(tasks);
    }
}