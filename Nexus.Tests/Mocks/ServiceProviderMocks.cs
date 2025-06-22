namespace Nexus.Tests.Mocks;

/// <summary>
/// Mock factory for IServiceProvider.
/// </summary>
public static class ServiceProviderMocks
{
    /// <summary>
    /// Creates a mock IServiceProvider with a handler for the specified request/response types.
    /// </summary>
    public static IServiceProvider CreateWithHandler<TRequest, TResponse>(IRequestHandler<TRequest, TResponse> handler)
        where TRequest : IRequest<TResponse>
    {
        var mock = new Mock<IServiceProvider>();
        
        // Mock GetService for the handler
        mock.Setup(x => x.GetService(typeof(IRequestHandler<TRequest, TResponse>)))
            .Returns(handler);
        
        // Mock GetService for pipeline behaviors (empty array)
        mock.Setup(x => x.GetService(typeof(IEnumerable<IPipelineBehavior<IRequest<TResponse>, TResponse>>)))
            .Returns(Array.Empty<IPipelineBehavior<IRequest<TResponse>, TResponse>>());
        
        return mock.Object;
    }

    /// <summary>
    /// Creates a mock IServiceProvider with a handler for requests without response.
    /// </summary>
    public static IServiceProvider CreateWithHandler<TRequest>(IRequestHandler<TRequest> handler)
        where TRequest : IRequest
    {
        var mock = new Mock<IServiceProvider>();
        
        // Mock GetService for the handler
        mock.Setup(x => x.GetService(typeof(IRequestHandler<TRequest>)))
            .Returns(handler);
        
        // Note: Requests without response don't use pipeline behaviors in the current implementation
        // So we don't need to mock pipeline behaviors for these requests
        
        return mock.Object;
    }

    /// <summary>
    /// Creates a mock IServiceProvider with notification handlers.
    /// </summary>
    public static IServiceProvider CreateWithNotificationHandlers<TNotification>(
        IEnumerable<INotificationHandler<TNotification>> handlers)
        where TNotification : INotification
    {
        var mock = new Mock<IServiceProvider>();
        
        // Mock GetService for notification handlers (using IEnumerable to avoid extension method)
        mock.Setup(x => x.GetService(typeof(IEnumerable<INotificationHandler<TNotification>>)))
            .Returns(handlers.Cast<object>().ToArray());
        
        return mock.Object;
    }

    /// <summary>
    /// Creates a mock IServiceProvider with pipeline behaviors.
    /// </summary>
    public static IServiceProvider CreateWithPipelineBehaviors<TRequest, TResponse>(
        IRequestHandler<TRequest, TResponse> handler,
        IEnumerable<IPipelineBehavior<TRequest, TResponse>> behaviors)
        where TRequest : IRequest<TResponse>
    {
        var mock = new Mock<IServiceProvider>();
        
        // Mock GetService for the handler
        mock.Setup(x => x.GetService(typeof(IRequestHandler<TRequest, TResponse>)))
            .Returns(handler);
        
        // Registrar behaviors do tipo correto IPipelineBehavior<TRequest, TResponse>
        mock.Setup(x => x.GetService(typeof(IEnumerable<IPipelineBehavior<TRequest, TResponse>>)))
            .Returns(behaviors.Cast<object>().ToArray());
        
        return mock.Object;
    }

    /// <summary>
    /// Creates a mock IServiceProvider with missing handler.
    /// </summary>
    public static IServiceProvider CreateWithMissingHandler<TRequest, TResponse>()
        where TRequest : IRequest<TResponse>
    {
        var mock = new Mock<IServiceProvider>();
        
        // Mock GetService to return null (missing handler)
        mock.Setup(x => x.GetService(typeof(IRequestHandler<TRequest, TResponse>)))
            .Returns((object?)null);
        
        // Mock GetService for pipeline behaviors (returns empty collection)
        mock.Setup(x => x.GetService(typeof(IEnumerable<IPipelineBehavior<IRequest<TResponse>, TResponse>>)))
            .Returns(Array.Empty<IPipelineBehavior<IRequest<TResponse>, TResponse>>());
        
        return mock.Object;
    }

    /// <summary>
    /// Creates a mock IServiceProvider with missing handler for requests without response.
    /// </summary>
    public static IServiceProvider CreateWithMissingHandler<TRequest>()
        where TRequest : IRequest
    {
        var mock = new Mock<IServiceProvider>();
        
        // Mock GetService to return null (missing handler)
        mock.Setup(x => x.GetService(typeof(IRequestHandler<TRequest>)))
            .Returns((object?)null);
        
        // Note: Requests without response don't use pipeline behaviors in the current implementation
        // So we don't need to mock pipeline behaviors for these requests
        
        return mock.Object;
    }

    /// <summary>
    /// Creates a mock IServiceProvider with no notification handlers.
    /// </summary>
    public static IServiceProvider CreateWithNoNotificationHandlers<TNotification>()
        where TNotification : INotification
    {
        var mock = new Mock<IServiceProvider>();
        
        // Mock GetService to return empty collection (no handlers)
        mock.Setup(x => x.GetService(typeof(IEnumerable<INotificationHandler<TNotification>>)))
            .Returns(Array.Empty<INotificationHandler<TNotification>>());
        
        return mock.Object;
    }
} 