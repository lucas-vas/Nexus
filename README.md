# NexusX

A .NET library for implementing the CQRS (Command Query Responsibility Segregation) pattern in a simple and efficient way.

## 🚀 Features

- ✅ **Complete CQRS implementation** - Commands, Queries and Notifications
- ✅ **Commands with and without responses** - Support for both patterns
- ✅ **Pipeline Behaviors** - Middleware for cross-cutting concerns
- ✅ **Native Dependency Injection** - Integration with Microsoft.Extensions.DependencyInjection
- ✅ **Thread-safe** - Safe for use in multi-threaded applications
- ✅ **Performance optimized** - Optimized reflection and type caching
- ✅ **Robust error handling** - Validations and well-defined exceptions

## 📦 Installation

```bash
dotnet add package NexusX
```

## 🔧 Configuration

### 1. Register in Program.cs

```csharp
using Nexus;

var builder = WebApplication.CreateBuilder(args);

// Register Nexus with the assembly containing your handlers
builder.Services.AddNexus(typeof(Program).Assembly);

var app = builder.Build();
```

### 2. For multiple assemblies

```csharp
builder.Services.AddNexus(
    typeof(Program).Assembly,
    typeof(YourCommand).Assembly
);
```

## 📝 Usage

### Commands and Queries with Response

```csharp
// 1. Create the Request
public class CreateUserCommand : IRequest<UserResponse>
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

// 2. Create the Handler
public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, UserResponse>
{
    public Task<UserResponse> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        // Your logic here
        var user = new UserResponse 
        { 
            Id = Guid.NewGuid(), 
            Name = request.Name, 
            Email = request.Email 
        };
        
        return Task.FromResult(user);
    }
}

// 3. Use in Controller
[ApiController]
[Route("[controller]")]
public class UsersController : ControllerBase
{
    private readonly INexus _nexus;
    
    public UsersController(INexus nexus)
    {
        _nexus = nexus;
    }
    
    [HttpPost]
    public async Task<UserResponse> Create(CreateUserCommand command)
    {
        return await _nexus.Send(command);
    }
}
```

### Commands without Response

```csharp
// 1. Create the Command
public class DeleteUserCommand : IRequest
{
    public Guid UserId { get; set; }
}

// 2. Create the Handler
public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand>
{
    public Task Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        // Your logic here - delete user from database
        Console.WriteLine($"Deleting user {request.UserId}");
        return Task.CompletedTask;
    }
}

// 3. Use in Controller
[HttpDelete("{id}")]
public async Task<IActionResult> Delete(Guid id)
{
    await _nexus.Send(new DeleteUserCommand { UserId = id });
    return NoContent();
}
```

### Notifications

```csharp
// 1. Create the Notification
public class UserCreatedNotification : INotification
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
}

// 2. Create the Handler
public class UserCreatedNotificationHandler : INotificationHandler<UserCreatedNotification>
{
    public Task Handle(UserCreatedNotification notification, CancellationToken cancellationToken)
    {
        // Send email, log, etc.
        Console.WriteLine($"User {notification.UserId} created. Email: {notification.Email}");
        return Task.CompletedTask;
    }
}

// 3. Publish the Notification
await _nexus.Publish(new UserCreatedNotification 
{ 
    UserId = user.Id, 
    Email = user.Email 
});
```

### Pipeline Behaviors

```csharp
// 1. Create the Behavior
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Executing {typeof(TRequest).Name}");
        
        var response = await next();
        
        Console.WriteLine($"Completed {typeof(TRequest).Name}");
        
        return response;
    }
}

// 2. Register the Behavior
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
```

## 🐛 Debug

To check if your handlers are being registered correctly:

```csharp
var app = builder.Build();

// List all registered handlers
app.Services.DebugRegisteredHandlers();
```

## 🔒 Security

The library includes several security validations:

- ✅ Null parameter validation
- ✅ Handler existence verification
- ✅ Reflection exception handling
- ✅ Public type validation
- ✅ Error handling in notifications

## 📄 License

MIT License - see the [LICENSE](LICENSE) file for details.

## 🤝 Contributing

Contributions are welcome! Please open an issue or pull request.

## 📞 Support

If you encounter any issues or have questions, please open an issue on GitHub. 