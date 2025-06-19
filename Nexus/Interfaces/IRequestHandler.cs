namespace Nexus.Interfaces;

/// <summary>
/// Interface for handlers that process requests and return responses.
/// </summary>
/// <typeparam name="TRequest">Type of the request to be processed.</typeparam>
/// <typeparam name="TResponse">Type of the response returned.</typeparam>
public interface IRequestHandler<in TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    /// <summary>
    /// Processes the request and returns the response.
    /// </summary>
    /// <param name="request">The request to be processed.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The processing response.</returns>
    Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
}

/// <summary>
/// Interface for handlers that process requests without returning a response.
/// </summary>
/// <typeparam name="TRequest">Type of the request to be processed.</typeparam>
public interface IRequestHandler<in TRequest> where TRequest : IRequest
{
    /// <summary>
    /// Processes the request without returning a response.
    /// </summary>
    /// <param name="request">The request to be processed.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task representing the asynchronous operation.</returns>
    Task Handle(TRequest request, CancellationToken cancellationToken);
}