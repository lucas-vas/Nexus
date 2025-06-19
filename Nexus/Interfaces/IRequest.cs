namespace Nexus.Interfaces;

/// <summary>
/// Base interface for all requests (Commands and Queries) in the CQRS pattern.
/// </summary>
/// <typeparam name="TResponse">Type of the response that the request will return.</typeparam>
public interface IRequest<out TResponse> { }

/// <summary>
/// Base interface for requests (Commands) that don't return a response in the CQRS pattern.
/// </summary>
public interface IRequest { }