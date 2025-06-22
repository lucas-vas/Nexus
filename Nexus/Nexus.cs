using Nexus.Interfaces;
using System.Reflection;
using System.Collections.Concurrent;
using System.Runtime.ExceptionServices;

namespace Nexus;

/// <summary>
/// Main implementation of the Nexus mediator for handling requests and notifications.
/// </summary>
public sealed class Nexus : INexus
{
    private readonly IServiceProvider _serviceProvider;
    // Cache para reflection de handlers
    private static readonly ConcurrentDictionary<Type, MethodInfo> HandlerMethodCache = new();

    /// <summary>
    /// Initializes a new instance of the Nexus class.
    /// </summary>
    /// <param name="serviceProvider">The service provider for dependency injection.</param>
    public Nexus(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    /// <summary>
    /// Sends a request and returns the response from the corresponding handler.
    /// </summary>
    /// <typeparam name="TResponse">Expected response type.</typeparam>
    /// <param name="request">The request to be processed.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>The handler response.</returns>
    /// <exception cref="ArgumentNullException">Thrown when request is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when no handler is registered or handler execution fails.</exception>
    public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));
        
        var requestType = request.GetType();
        var handlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, typeof(TResponse));

        // Validação de tipo de request
        if (!typeof(IRequest<TResponse>).IsAssignableFrom(requestType))
            throw new ArgumentException($"O tipo {requestType.Name} não implementa IRequest<{typeof(TResponse).Name}>");

        var handler = GetHandler(handlerType, requestType.Name);
        var handlerMethod = HandlerMethodCache.GetOrAdd(handlerType, GetHandlerMethod);

        // Buscar behaviors do tipo IPipelineBehavior<TRequest, TResponse> via reflection
        var pipelineBehaviorType = typeof(IPipelineBehavior<,>).MakeGenericType(requestType, typeof(TResponse));
        var behaviorsEnumerableType = typeof(IEnumerable<>).MakeGenericType(pipelineBehaviorType);
        var behaviorsObj = _serviceProvider.GetService(behaviorsEnumerableType);
        var behaviors = (behaviorsObj as IEnumerable<object> ?? Enumerable.Empty<object>())
            .ToArray();

        if (!behaviors.Any())
        {
            System.Diagnostics.Debug.WriteLine($"[Nexus] Nenhum pipeline behavior registrado para {requestType.Name}");
        }

        var handlerDelegate = new RequestHandlerDelegate<TResponse>(async () => 
        {
            try
            {
                var result = handlerMethod.Invoke(handler, new object[] { request, cancellationToken });
                var taskResult = result as Task<TResponse>;
                
                if (taskResult == null)
                {
                    throw new InvalidOperationException($"Handler '{handlerType.Name}' returned null");
                }

                var response = await taskResult;
                if (response == null)
                {
                    throw new InvalidOperationException($"Handler '{handlerType.Name}' returned null response");
                }

                return response;
            }
            catch (TargetInvocationException tie) when (tie.InnerException is OperationCanceledException oce)
            {
                // Propaga OperationCanceledException diretamente
                System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(oce).Throw();
                throw; // nunca será executado
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (AggregateException)
            {
                throw;
            }
            catch (Exception ex) when (ex is not InvalidOperationException)
            {
                var innerException = ex.InnerException ?? ex;
                throw new InvalidOperationException($"Error executing handler '{handlerType.Name}': {innerException.Message}", innerException);
            }
        });

        try
        {
            var pipeline = behaviors.Aggregate(
                (RequestHandlerDelegate<TResponse>)handlerDelegate,
                (next, behaviorObj) =>
                {
                    return () =>
                    {
                        // Usar reflection para invocar Handle
                        var behaviorType = behaviorObj.GetType();
                        var handleMethod = behaviorType.GetMethod("Handle");
                        if (handleMethod == null)
                            throw new InvalidOperationException($"Pipeline behavior '{behaviorType.Name}' não implementa Handle");
                        var parameters = handleMethod.GetParameters();
                        if (parameters.Length != 3)
                            throw new InvalidOperationException($"Pipeline behavior '{behaviorType.Name}' Handle deve ter 3 parâmetros");
                        try
                        {
                            var result = handleMethod.Invoke(behaviorObj, new object[] { request, next, cancellationToken });
                            if (result == null)
                                throw new NullReferenceException($"Pipeline behavior '{behaviorType.Name}' retornou null");
                            return (Task<TResponse>)result;
                        }
                        catch (TargetInvocationException tie) when (tie.InnerException != null)
                        {
                            // Propaga a exceção original lançada pelo behavior
                            ExceptionDispatchInfo.Capture(tie.InnerException).Throw();
                            throw; // nunca será executado
                        }
                    };
                }
            );
            
            return await pipeline();
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (AggregateException)
        {
            throw;
        }
        catch (NullReferenceException)
        {
            throw;
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            throw new InvalidOperationException($"Error in pipeline execution: {ex.Message}", ex);
        }
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
        if (request == null) throw new ArgumentNullException(nameof(request));
        
        var requestType = request.GetType();
        var handlerType = typeof(IRequestHandler<>).MakeGenericType(requestType);
        if (!typeof(IRequest).IsAssignableFrom(requestType))
            throw new ArgumentException($"O tipo {requestType.Name} não implementa IRequest");
        var handler = GetHandler(handlerType, requestType.Name);
        var handlerMethod = HandlerMethodCache.GetOrAdd(handlerType, GetHandlerMethod);

        try
        {
            var result = handlerMethod.Invoke(handler, new object[] { request, cancellationToken });
            return result as Task ?? Task.CompletedTask;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (AggregateException)
        {
            throw;
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            var innerException = ex.InnerException ?? ex;
            throw new InvalidOperationException($"Error executing handler '{handlerType.Name}': {innerException.Message}", innerException);
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
        if (notification == null) throw new ArgumentNullException(nameof(notification));
        
        var notificationHandlersObj = _serviceProvider.GetService(typeof(IEnumerable<INotificationHandler<TNotification>>));
        var notificationHandlers = (notificationHandlersObj as IEnumerable<object> ?? Enumerable.Empty<object>())
            .OfType<INotificationHandler<TNotification>>()
            .ToArray();
        if (!notificationHandlers.Any())
        {
            System.Diagnostics.Debug.WriteLine($"[Nexus] Nenhum handler de notificação registrado para {typeof(TNotification).Name}");
            return;
        }

        var tasks = notificationHandlers.Select(handler => 
        {
            try
            {
                return handler.Handle(notification, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
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
        var handler = _serviceProvider.GetService(handlerType);
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
    private static MethodInfo GetHandlerMethod(Type handlerType)
    {
        var handlerMethod = handlerType.GetMethod("Handle");
        if (handlerMethod == null)
        {
            throw new InvalidOperationException($"Handler '{handlerType.Name}' does not implement the Handle method");
        }
        return handlerMethod;
    }
}