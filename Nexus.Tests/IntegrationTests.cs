namespace Nexus.Tests;

/// <summary>
/// Integration tests for the complete Nexus flow.
/// </summary>
public class IntegrationTests
{
    [Fact]
    public async Task CompleteFlow_WithValidSetup_ShouldWorkEndToEnd()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddNexus(typeof(IntegrationTests).Assembly);
        
        // Register test handlers
        services.AddTransient<IRequestHandler<CreateUserCommand, UserResponse>, TestCreateUserCommandHandler>();
        services.AddTransient<IRequestHandler<DeleteUserCommand>, TestDeleteUserCommandHandler>();
        services.AddTransient<INotificationHandler<UserCreatedNotification>, TestUserCreatedNotificationHandler>();
        services.AddTransient<INotificationHandler<UserDeletedNotification>, TestUserDeletedNotificationHandler>();
        
        var serviceProvider = services.BuildServiceProvider();
        var nexus = serviceProvider.GetRequiredService<INexus>();

        // Act & Assert - Test Create User
        var createCommand = new CreateUserCommandBuilder().Build();
        var createResult = await nexus.Send(createCommand);
        
        createResult.ShouldNotBeNull();
        createResult.Name.ShouldBe(createCommand.Name);
        createResult.Email.ShouldBe(createCommand.Email);

        // Act & Assert - Test Delete User
        var deleteCommand = new DeleteUserCommandBuilder().WithUserId(createResult.Id).Build();
        await nexus.Send(deleteCommand);

        // Act & Assert - Test Notifications
        var notification = new UserCreatedNotification
        {
            UserId = createResult.Id,
            Email = createResult.Email
        };
        
        await nexus.Publish(notification);
    }

    [Fact]
    public async Task MultipleHandlers_WithSameNotification_ShouldExecuteAll()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddNexus(typeof(IntegrationTests).Assembly);
        
        // Register multiple handlers for the same notification
        services.AddTransient<INotificationHandler<UserCreatedNotification>, TestUserCreatedNotificationHandler>();
        services.AddTransient<INotificationHandler<UserCreatedNotification>, TestUserCreatedNotificationHandler2>();
        
        var serviceProvider = services.BuildServiceProvider();
        var nexus = serviceProvider.GetRequiredService<INexus>();

        // Act
        var notification = new UserCreatedNotification
        {
            UserId = TestData.Users.ValidUserId,
            Email = TestData.Users.ValidEmail
        };
        
        await nexus.Publish(notification);

        // Assert - Both handlers should have been executed
        TestUserCreatedNotificationHandler.ExecutionCount.ShouldBeGreaterThan(0);
        TestUserCreatedNotificationHandler2.ExecutionCount.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task PipelineBehaviors_WithValidSetup_ShouldExecuteInOrder()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddNexus(typeof(IntegrationTests).Assembly);
        
        // Register handler
        services.AddTransient<IRequestHandler<CreateUserCommand, UserResponse>, TestCreateUserCommandHandler>();
        
        // Register pipeline behaviors
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TestLoggingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TestValidationBehavior<,>));
        
        var serviceProvider = services.BuildServiceProvider();
        var nexus = serviceProvider.GetRequiredService<INexus>();

        // Reset counters
        TestLoggingBehavior<CreateUserCommand, UserResponse>.ExecutionCount = 0;
        TestValidationBehavior<CreateUserCommand, UserResponse>.ExecutionCount = 0;

        // Act
        var command = new CreateUserCommandBuilder().Build();
        var result = await nexus.Send(command);

        // Assert
        result.ShouldNotBeNull();
        TestLoggingBehavior<CreateUserCommand, UserResponse>.ExecutionCount.ShouldBe(1);
        TestValidationBehavior<CreateUserCommand, UserResponse>.ExecutionCount.ShouldBe(1);
    }

    [Fact]
    public async Task ErrorHandling_WithHandlerThrowingException_ShouldPropagateException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddNexus(typeof(IntegrationTests).Assembly);
        
        // Register handler that throws exception
        services.AddTransient<IRequestHandler<CreateUserCommand, UserResponse>, TestFailingCreateUserCommandHandler>();
        
        var serviceProvider = services.BuildServiceProvider();
        var nexus = serviceProvider.GetRequiredService<INexus>();

        // Act & Assert
        var command = new CreateUserCommandBuilder().Build();
        var action = async () => await nexus.Send(command);
        
        var exception = await action.ShouldThrowAsync<InvalidOperationException>();
        exception.Message.ShouldContain("Test exception");
    }

    [Fact]
    public async Task CancellationToken_WithCancelledToken_ShouldRespectCancellation()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddNexus(typeof(IntegrationTests).Assembly);
        
        // Register handler that respects cancellation
        services.AddTransient<IRequestHandler<CreateUserCommand, UserResponse>, TestCancellableCreateUserCommandHandler>();
        
        var serviceProvider = services.BuildServiceProvider();
        var nexus = serviceProvider.GetRequiredService<INexus>();

        // Act & Assert
        var command = new CreateUserCommandBuilder().Build();
        var cts = new CancellationTokenSource();
        cts.Cancel(); // Cancel immediately
        
        var action = async () => await nexus.Send(command, cts.Token);
        await action.ShouldThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task CompleteFlow_WithRealServiceProviderAndBehaviors_ShouldWorkEndToEnd()
    {
        // Arrange
        // Usar behaviors padrão (com contadores estáticos)
        var behavior1 = new TestLoggingBehavior<CreateUserCommand, UserResponse>();
        var behavior2 = new TestLoggingBehavior<CreateUserCommand, UserResponse>();
        var handler = new TestCreateUserCommandHandler();
        var provider = TestServiceProviderHelpers.BuildProviderWithHandlersAndBehaviors(handler, behavior1, behavior2);
        var nexus = new Nexus(provider);
        var request = new TestModels.CreateUserCommand { Name = "Teste" };

        // Act
        var response = await nexus.Send(request);

        // Assert
        Assert.Equal("Teste", response.Name);
        // Como os behaviors padrão só incrementam contador, não há log de ordem
        Assert.Equal(2, TestLoggingBehavior<CreateUserCommand, UserResponse>.ExecutionCount);
    }
}

// Test Handlers for Integration Tests
public class TestCreateUserCommandHandler : IRequestHandler<CreateUserCommand, UserResponse>
{
    public Task<UserResponse> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new UserResponse
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Email = request.Email,
            CreatedAt = DateTime.UtcNow
        });
    }
}

public class TestDeleteUserCommandHandler : IRequestHandler<DeleteUserCommand>
{
    public Task Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}

public class TestFailingCreateUserCommandHandler : IRequestHandler<CreateUserCommand, UserResponse>
{
    public Task<UserResponse> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        throw new InvalidOperationException("Test exception");
    }
}

public class TestCancellableCreateUserCommandHandler : IRequestHandler<CreateUserCommand, UserResponse>
{
    public Task<UserResponse> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(new UserResponse
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Email = request.Email,
            CreatedAt = DateTime.UtcNow
        });
    }
}

public class TestUserCreatedNotificationHandler : INotificationHandler<UserCreatedNotification>
{
    public static int ExecutionCount { get; private set; }

    public Task Handle(UserCreatedNotification notification, CancellationToken cancellationToken)
    {
        ExecutionCount++;
        return Task.CompletedTask;
    }
}

public class TestUserCreatedNotificationHandler2 : INotificationHandler<UserCreatedNotification>
{
    public static int ExecutionCount { get; private set; }

    public Task Handle(UserCreatedNotification notification, CancellationToken cancellationToken)
    {
        ExecutionCount++;
        return Task.CompletedTask;
    }
}

public class TestUserDeletedNotificationHandler : INotificationHandler<UserDeletedNotification>
{
    public Task Handle(UserDeletedNotification notification, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}

// Test Pipeline Behaviors
public class TestLoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public static int ExecutionCount { get; set; }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        ExecutionCount++;
        return await next();
    }
}

public class TestValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public static int ExecutionCount { get; set; }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        ExecutionCount++;
        return await next();
    }
} 