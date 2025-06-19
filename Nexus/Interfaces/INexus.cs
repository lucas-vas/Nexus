namespace Nexus.Interfaces;

/// <summary>
/// Main interface of Nexus for sending requests and publishing notifications.
/// </summary>
public interface INexus
{
    /// <summary>
    /// Sends a request and returns the response from the corresponding handler.
    /// </summary>
    /// <typeparam name="TResponse">Expected response type.</typeparam>
    /// <param name="request">The request to be processed.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>The handler response.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no handler is registered for the request type.</exception>
    Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Sends a request that doesn't return a response.
    /// </summary>
    /// <param name="request">The request to be processed.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>Task representing the asynchronous operation.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no handler is registered for the request type.</exception>
    Task Send(IRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Publishes a notification to all registered handlers.
    /// </summary>
    /// <typeparam name="TNotification">Notification type.</typeparam>
    /// <param name="notification">The notification to be published.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>Task representing the asynchronous operation.</returns>
    Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default) where TNotification : INotification;
}