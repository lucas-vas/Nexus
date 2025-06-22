using Nexus.Interfaces;

namespace Nexus.Tests.TestModels;

// Test Requests with Response
public class CreateUserCommand : IRequest<UserResponse>
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class GetUserQuery : IRequest<UserResponse>
{
    public Guid UserId { get; set; }
}

public class GetUserListQuery : IRequest<UserListResponse>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }
}

public class UpdateUserCommand : IRequest<UserResponse>
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

// Test Requests without Response
public class DeleteUserCommand : IRequest
{
    public Guid UserId { get; set; }
}

public class LogUserActionCommand : IRequest
{
    public Guid UserId { get; set; }
    public string Action { get; set; } = string.Empty;
}

// Test Responses
public class UserResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class UserListResponse
{
    public List<UserResponse> Users { get; set; } = new();
    public int TotalCount { get; set; }
}

// Test Notifications
public class UserCreatedNotification : INotification
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
}

public class UserDeletedNotification : INotification
{
    public Guid UserId { get; set; }
    public DateTime DeletedAt { get; set; }
} 