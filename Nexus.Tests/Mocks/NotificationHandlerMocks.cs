using Moq;
using Nexus.Interfaces;
using Nexus.Tests.Common;
using Nexus.Tests.TestModels;

namespace Nexus.Tests.Mocks;

/// <summary>
/// Mock setups for notification handlers.
/// </summary>
public static class NotificationHandlerMocks
{
    /// <summary>
    /// Creates a mock for UserCreatedNotificationHandler.
    /// </summary>
    /// <returns>The mock handler.</returns>
    public static INotificationHandler<UserCreatedNotification> UserCreatedNotificationHandler()
    {
        var mock = new Mock<INotificationHandler<UserCreatedNotification>>();
        
        mock.Setup(x => x.Handle(It.IsAny<UserCreatedNotification>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        
        return mock.Object;
    }

    /// <summary>
    /// Creates a mock for UserCreatedNotificationHandler that throws an exception.
    /// </summary>
    /// <param name="exception">The exception to throw.</param>
    /// <returns>The mock handler.</returns>
    public static INotificationHandler<UserCreatedNotification> UserCreatedNotificationHandlerWithException(Exception exception)
    {
        var mock = new Mock<INotificationHandler<UserCreatedNotification>>();
        
        mock.Setup(x => x.Handle(It.IsAny<UserCreatedNotification>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);
        
        return mock.Object;
    }

    /// <summary>
    /// Creates a mock for UserDeletedNotificationHandler.
    /// </summary>
    /// <returns>The mock handler.</returns>
    public static INotificationHandler<UserDeletedNotification> UserDeletedNotificationHandler()
    {
        var mock = new Mock<INotificationHandler<UserDeletedNotification>>();
        
        mock.Setup(x => x.Handle(It.IsAny<UserDeletedNotification>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        
        return mock.Object;
    }

    /// <summary>
    /// Creates multiple mock handlers for UserCreatedNotification.
    /// </summary>
    /// <param name="count">Number of handlers to create.</param>
    /// <returns>Array of mock handlers.</returns>
    public static INotificationHandler<UserCreatedNotification>[] MultipleUserCreatedNotificationHandlers(int count = 3)
    {
        var handlers = new INotificationHandler<UserCreatedNotification>[count];
        
        for (int i = 0; i < count; i++)
        {
            handlers[i] = UserCreatedNotificationHandler();
        }
        
        return handlers;
    }

    /// <summary>
    /// Creates multiple mock handlers for UserCreatedNotification with some throwing exceptions.
    /// </summary>
    /// <param name="totalCount">Total number of handlers to create.</param>
    /// <param name="exceptionCount">Number of handlers that should throw exceptions.</param>
    /// <returns>Array of mock handlers.</returns>
    public static INotificationHandler<UserCreatedNotification>[] MultipleUserCreatedNotificationHandlersWithExceptions(int totalCount = 5, int exceptionCount = 2)
    {
        var handlers = new INotificationHandler<UserCreatedNotification>[totalCount];
        
        for (int i = 0; i < totalCount; i++)
        {
            if (i < exceptionCount)
            {
                handlers[i] = UserCreatedNotificationHandlerWithException(new InvalidOperationException($"Handler {i} error"));
            }
            else
            {
                handlers[i] = UserCreatedNotificationHandler();
            }
        }
        
        return handlers;
    }
} 