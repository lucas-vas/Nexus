namespace Nexus.Tests.Common;

/// <summary>
/// Common test data for reuse across tests.
/// </summary>
public static class TestData
{
    public static class Users
    {
        public static readonly Guid ValidUserId = Guid.NewGuid();
        public static readonly string ValidName = "John Doe";
        public static readonly string ValidEmail = "john.doe@example.com";
        public static readonly string InvalidEmail = "invalid-email";
    }

    public static class Commands
    {
        public static readonly string ValidCommandName = "CreateUserCommand";
        public static readonly string ValidQueryName = "GetUserQuery";
    }

    public static class Notifications
    {
        public static readonly string ValidNotificationName = "UserCreatedNotification";
    }

    public static class Exceptions
    {
        public static readonly string HandlerNotFoundMessage = "No handler registered for request type";
        public static readonly string HandlerMethodNotFoundMessage = "does not implement the Handle method";
    }
} 