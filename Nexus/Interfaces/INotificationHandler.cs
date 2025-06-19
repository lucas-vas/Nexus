namespace Nexus.Interfaces;

/// <summary>
/// Interface for handlers that process notifications.
/// </summary>
/// <typeparam name="TNotification">Type of the notification to be processed.</typeparam>
public interface INotificationHandler<in TNotification> where TNotification : INotification
{
    /// <summary>
    /// Handles the notification.
    /// </summary>
    /// <param name="notification">The notification to be handled.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task representing the asynchronous operation.</returns>
    Task Handle(TNotification notification, CancellationToken cancellationToken);
}