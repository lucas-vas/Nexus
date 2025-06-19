using Nexus.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Nexus;

/// <summary>
/// Main implementation of the Nexus mediator for handling requests and notifications.
/// </summary>
/// <param name="serviceProvider">The service provider for dependency injection.</param>
public sealed class Nexus(IServiceProvider serviceProvider) : INexus
{
    /// <summary>
    /// Sends a request and returns the response from the corresponding handler.
    /// </summary>
    /// <typeparam name="TResponse">Expected response type.</typeparam>
    /// <param name="request">The request to be processed.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>The handler response.</returns>
    /// <exception cref="ArgumentNullException">Thrown when request is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when no handler is registered or handler execution fails.</exception>
    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));
        
        var requestType = request.GetType();
        var handlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, typeof(TResponse));

        var handler = GetHandler(handlerType, requestType.Name);
        var handlerMethod = GetHandlerMethod(handlerType);

        var behaviors = serviceProvider.GetServices<IPipelineBehavior<IRequest<TResponse>, TResponse>>().Reverse();

        var handlerDelegate = new RequestHandlerDelegate<TResponse>(() => 
        {
            try
            {
                var result = handlerMethod.Invoke(handler, [request, cancellationToken]);
                return result as Task<TResponse> ?? throw new InvalidOperationException($"Handler '{handlerType.Name}' returned null");
            }
            catch (Exception ex) when (ex is not InvalidOperationException)
            {
                throw new InvalidOperationException($"Error executing handler '{handlerType.Name}': {ex.Message}", ex);
            }
        });

        var pipeline = behaviors.Aggregate(
            (RequestHandlerDelegate<TResponse>)handlerDelegate,
            (next, pipeline) => () => pipeline.Handle((IRequest<TResponse>)request, next, cancellationToken)
        );

        return pipeline();
    }

    /// <summary>
    /// Sends a request that doesn't return a response.
    /// </summary>
    /// <param name="request">The request to be processed.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>Task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when request is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when no handler is registered or handler execution fails.</exception>
    public Task Send(IRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));
        
        var requestType = request.GetType();
        var handlerType = typeof(IRequestHandler<>).MakeGenericType(requestType);

        var handler = GetHandler(handlerType, requestType.Name);
        var handlerMethod = GetHandlerMethod(handlerType);

        try
        {
            var result = handlerMethod.Invoke(handler, [request, cancellationToken]);
            return result as Task ?? Task.CompletedTask;
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            throw new InvalidOperationException($"Error executing handler '{handlerType.Name}': {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Publishes a notification to all registered handlers.
    /// </summary>
    /// <typeparam name="TNotification">Notification type.</typeparam>
    /// <param name="notification">The notification to be published.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>Task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when notification is null.</exception>
    public async Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
        where TNotification : INotification
    {
        ArgumentNullException.ThrowIfNull(notification, nameof(notification));
        
        var notificationHandlers = serviceProvider.GetServices<INotificationHandler<TNotification>>();

        if (!notificationHandlers.Any())
        {
            // Not an error to have no handlers for notifications
            return;
        }

        var tasks = notificationHandlers.Select(handler => 
        {
            try
            {
                return handler.Handle(notification, cancellationToken);
            }
            catch (Exception ex)
            {
                // Log the error but don't fail the entire operation
                return Task.FromException(ex);
            }
        });
        
        await Task.WhenAll(tasks);
    }

    /// <summary>
    /// Gets a handler instance from the service provider.
    /// </summary>
    /// <param name="handlerType">The handler type to retrieve.</param>
    /// <param name="requestTypeName">The name of the request type for error messages.</param>
    /// <returns>The handler instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no handler is registered.</exception>
    private object GetHandler(Type handlerType, string requestTypeName)
    {
        var handler = serviceProvider.GetService(handlerType);
        if (handler == null)
        {
            throw new InvalidOperationException($"No handler registered for request type '{requestTypeName}'");
        }
        return handler;
    }

    /// <summary>
    /// Gets the Handle method from a handler type.
    /// </summary>
    /// <param name="handlerType">The handler type.</param>
    /// <returns>The Handle method info.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the Handle method is not found.</exception>
    private MethodInfo GetHandlerMethod(Type handlerType)
    {
        var handlerMethod = handlerType.GetMethod("Handle");
        if (handlerMethod == null)
        {
            throw new InvalidOperationException($"Handler '{handlerType.Name}' does not implement the Handle method");
        }
        return handlerMethod;
    }
}