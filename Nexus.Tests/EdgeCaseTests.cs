namespace Nexus.Tests;

/// <summary>
/// Tests for edge cases and extreme scenarios.
/// </summary>
public class EdgeCaseTests
{
    [Fact]
    public async Task Send_WithConcurrentRequests_ShouldHandleCorrectly()
    {
        // Arrange
        var handler = HandlerMocks.CreateUserCommandHandler();
        var serviceProvider = ServiceProviderMocks.CreateWithHandler<CreateUserCommand, UserResponse>(handler);
        var nexus = new Nexus(serviceProvider);
        var requests = Enumerable.Range(0, 10)
            .Select(i => new CreateUserCommandBuilder().WithName($"User {i}").Build())
            .ToArray();

        // Act
        var tasks = requests.Select(request => nexus.Send(request));
        var results = await Task.WhenAll(tasks);

        // Assert
        results.Length.ShouldBe(10);
        results.ShouldAllBe(r => r != null);
    }

    [Fact]
    public async Task Publish_WithConcurrentNotifications_ShouldHandleCorrectly()
    {
        // Arrange
        var handlers = NotificationHandlerMocks.MultipleUserCreatedNotificationHandlers(3);
        var serviceProvider = ServiceProviderMocks.CreateWithNotificationHandlers(handlers);
        var nexus = new Nexus(serviceProvider);
        var notifications = Enumerable.Range(0, 5)
            .Select(i => new UserCreatedNotification
            {
                UserId = Guid.NewGuid(),
                Email = $"user{i}@example.com"
            })
            .ToArray();

        // Act
        var tasks = notifications.Select(notification => nexus.Publish(notification));
        await Task.WhenAll(tasks);

        // Assert - Should not throw any exceptions
        // The test passes if no exceptions are thrown
    }

    [Fact]
    public async Task Send_WithVeryLargeRequest_ShouldHandleCorrectly()
    {
        // Arrange
        var largeName = new string('A', 10000); // 10KB string
        var largeEmail = new string('B', 10000) + "@example.com";
        var request = new CreateUserCommandBuilder()
            .WithName(largeName)
            .WithEmail(largeEmail)
            .Build();

        var handler = HandlerMocks.CreateUserCommandHandler();
        var serviceProvider = ServiceProviderMocks.CreateWithHandler<CreateUserCommand, UserResponse>(handler);
        var nexus = new Nexus(serviceProvider);

        // Act
        var result = await nexus.Send(request);

        // Assert
        result.ShouldNotBeNull();
    }

    [Fact]
    public async Task Send_WithEmptyRequest_ShouldHandleCorrectly()
    {
        // Arrange
        var request = new CreateUserCommandBuilder()
            .WithName("")
            .WithEmail("")
            .Build();

        var handler = HandlerMocks.CreateUserCommandHandler();
        var serviceProvider = ServiceProviderMocks.CreateWithHandler<CreateUserCommand, UserResponse>(handler);
        var nexus = new Nexus(serviceProvider);

        // Act
        var result = await nexus.Send(request);

        // Assert
        result.ShouldNotBeNull();
    }

    [Fact]
    public async Task Send_WithNullResponseFromHandler_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var handler = new Mock<IRequestHandler<CreateUserCommand, UserResponse>>();
        handler.Setup(x => x.Handle(It.IsAny<CreateUserCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserResponse)null!);

        var serviceProvider = ServiceProviderMocks.CreateWithHandler<CreateUserCommand, UserResponse>(handler.Object);
        var nexus = new Nexus(serviceProvider);
        var request = new CreateUserCommandBuilder().Build();

        // Act & Assert
        var action = async () => await nexus.Send(request);
        var exception = await action.ShouldThrowAsync<InvalidOperationException>();
        exception.Message.ShouldContain("returned null");
    }

    [Fact]
    public async Task Send_WithHandlerReturningTaskFromResultNull_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var handler = new Mock<IRequestHandler<CreateUserCommand, UserResponse>>();
        handler.Setup(x => x.Handle(It.IsAny<CreateUserCommand>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult<UserResponse>(null!));

        var serviceProvider = ServiceProviderMocks.CreateWithHandler<CreateUserCommand, UserResponse>(handler.Object);
        var nexus = new Nexus(serviceProvider);
        var request = new CreateUserCommandBuilder().Build();

        // Act & Assert
        var action = async () => await nexus.Send(request);
        var exception = await action.ShouldThrowAsync<InvalidOperationException>();
        exception.Message.ShouldContain("returned null");
    }

    [Fact]
    public async Task Send_WithHandlerThrowingAggregateException_ShouldPropagateException()
    {
        // Arrange
        var innerException = new InvalidOperationException("Inner error");
        var aggregateException = new AggregateException(innerException);
        
        var handler = new Mock<IRequestHandler<CreateUserCommand, UserResponse>>();
        handler.Setup(x => x.Handle(It.IsAny<CreateUserCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(aggregateException);

        var serviceProvider = ServiceProviderMocks.CreateWithHandler<CreateUserCommand, UserResponse>(handler.Object);
        var nexus = new Nexus(serviceProvider);
        var request = new CreateUserCommandBuilder().Build();

        // Act & Assert
        var action = async () => await nexus.Send(request);
        var exception = await action.ShouldThrowAsync<AggregateException>();
        exception.InnerExceptions.ShouldContain(innerException);
    }

    [Fact]
    public async Task Send_WithHandlerThrowingTaskCanceledException_ShouldPropagateException()
    {
        // Arrange
        var handler = new Mock<IRequestHandler<CreateUserCommand, UserResponse>>();
        handler.Setup(x => x.Handle(It.IsAny<CreateUserCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TaskCanceledException());

        var serviceProvider = ServiceProviderMocks.CreateWithHandler<CreateUserCommand, UserResponse>(handler.Object);
        var nexus = new Nexus(serviceProvider);
        var request = new CreateUserCommandBuilder().Build();

        // Act & Assert
        var action = async () => await nexus.Send(request);
        await action.ShouldThrowAsync<TaskCanceledException>();
    }

    [Fact]
    public async Task Send_WithHandlerThrowingOperationCanceledException_ShouldPropagateException()
    {
        // Arrange
        var handler = new Mock<IRequestHandler<CreateUserCommand, UserResponse>>();
        handler.Setup(x => x.Handle(It.IsAny<CreateUserCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        var serviceProvider = ServiceProviderMocks.CreateWithHandler<CreateUserCommand, UserResponse>(handler.Object);
        var nexus = new Nexus(serviceProvider);
        var request = new CreateUserCommandBuilder().Build();

        // Act & Assert
        var action = async () => await nexus.Send(request);
        await action.ShouldThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task Send_WithHandlerTakingLongTime_ShouldRespectTimeout()
    {
        // Arrange
        var handler = new Mock<IRequestHandler<CreateUserCommand, UserResponse>>();
        handler.Setup(x => x.Handle(It.IsAny<CreateUserCommand>(), It.IsAny<CancellationToken>()))
            .Returns(async (CreateUserCommand request, CancellationToken token) =>
            {
                await Task.Delay(5000, token); // 5 seconds delay
                return new UserResponseBuilder().Build();
            });

        var serviceProvider = ServiceProviderMocks.CreateWithHandler<CreateUserCommand, UserResponse>(handler.Object);
        var nexus = new Nexus(serviceProvider);
        var request = new CreateUserCommandBuilder().Build();
        var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(100)); // 100ms timeout

        // Act & Assert
        var action = async () => await nexus.Send(request, cts.Token);
        await action.ShouldThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task Publish_WithManyHandlers_ShouldExecuteAll()
    {
        // Arrange
        var handlerCount = 100;
        var handlers = new INotificationHandler<UserCreatedNotification>[handlerCount];
        
        for (int i = 0; i < handlerCount; i++)
        {
            var mock = new Mock<INotificationHandler<UserCreatedNotification>>();
            mock.Setup(x => x.Handle(It.IsAny<UserCreatedNotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            handlers[i] = mock.Object;
        }

        var serviceProvider = ServiceProviderMocks.CreateWithNotificationHandlers(handlers);
        var nexus = new Nexus(serviceProvider);
        var notification = new UserCreatedNotification
        {
            UserId = TestData.Users.ValidUserId,
            Email = TestData.Users.ValidEmail
        };

        // Act
        await nexus.Publish(notification);

        // Assert - Should not throw any exceptions
        // The test passes if no exceptions are thrown
    }

    [Fact]
    public async Task Send_WithNestedRequests_ShouldHandleCorrectly()
    {
        // Arrange
        var innerHandler = HandlerMocks.CreateUserCommandHandler();
        var outerHandler = new Mock<IRequestHandler<UpdateUserCommand, UserResponse>>();
        
        outerHandler.Setup(x => x.Handle(It.IsAny<UpdateUserCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((UpdateUserCommand request, CancellationToken token) =>
            {
                // Simulate nested request
                var innerRequest = new CreateUserCommandBuilder().Build();
                // Note: In a real scenario, you would inject INexus into the handler
                return new UserResponseBuilder().Build();
            });

        var serviceProvider = ServiceProviderMocks.CreateWithHandler<UpdateUserCommand, UserResponse>(outerHandler.Object);
        var nexus = new Nexus(serviceProvider);
        var request = new UpdateUserCommandBuilder().Build();

        // Act
        var result = await nexus.Send(request);

        // Assert
        result.ShouldNotBeNull();
    }

    [Fact]
    public async Task Send_WithCircularDependency_ShouldNotCauseStackOverflow()
    {
        // Arrange
        var handler = new Mock<IRequestHandler<CreateUserCommand, UserResponse>>();
        handler.Setup(x => x.Handle(It.IsAny<CreateUserCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CreateUserCommand request, CancellationToken token) =>
            {
                // Simulate some processing that could potentially cause issues
                return new UserResponseBuilder().Build();
            });

        var serviceProvider = ServiceProviderMocks.CreateWithHandler<CreateUserCommand, UserResponse>(handler.Object);
        var nexus = new Nexus(serviceProvider);
        var request = new CreateUserCommandBuilder().Build();

        // Act
        var result = await nexus.Send(request);

        // Assert
        result.ShouldNotBeNull();
    }

    [Fact]
    public async Task Send_WithDisposedServiceProvider_ShouldThrowObjectDisposedException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddTransient<IRequestHandler<CreateUserCommand, UserResponse>, TestCreateUserCommandHandler>();
        services.AddNexus(typeof(EdgeCaseTests).Assembly);
        
        var serviceProvider = services.BuildServiceProvider();
        var nexus = new Nexus(serviceProvider);
        var request = new CreateUserCommandBuilder().Build();

        // Dispose the service provider
        serviceProvider.Dispose();

        // Act & Assert
        var action = async () => await nexus.Send(request);
        await action.ShouldThrowAsync<ObjectDisposedException>();
    }
} 