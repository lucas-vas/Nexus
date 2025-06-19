namespace Nexus.Interfaces;

/// <summary>
/// Delegate for request handler execution in the pipeline.
/// </summary>
/// <typeparam name="TResponse">Type of the response.</typeparam>
/// <returns>Task containing the response.</returns>
public delegate Task<TResponse> RequestHandlerDelegate<TResponse>();

/// <summary>
/// Interface for pipeline behaviors that can intercept and modify request processing.
/// </summary>
/// <typeparam name="TRequest">Type of the request to be processed.</typeparam>
/// <typeparam name="TResponse">Type of the response.</typeparam>
public interface IPipelineBehavior<in TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    /// <summary>
    /// Handles the request in the pipeline.
    /// </summary>
    /// <param name="request">The request to be processed.</param>
    /// <param name="next">The next handler in the pipeline.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The response from the pipeline.</returns>
    Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken);
}