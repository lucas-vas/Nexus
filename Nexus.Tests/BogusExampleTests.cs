namespace Nexus.Tests;

/// <summary>
/// Example tests demonstrating the use of Bogus for generating random test data.
/// </summary>
public class BogusExampleTests
{
    [Fact]
    public async Task Send_WithRandomUserData_ShouldReturnRandomResponse()
    {
        // Arrange
        var randomUserResponse = FakeData.GenerateUserResponse();
        var handler = HandlerMocks.CreateUserCommandHandler(randomUserResponse);
        var serviceProvider = ServiceProviderMocks.CreateWithHandler<CreateUserCommand, UserResponse>(handler);
        var nexus = new Nexus(serviceProvider);
        
        // Generate random request data
        var request = FakeData.GenerateCreateUserCommand();

        // Act
        var result = await nexus.Send(request);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(randomUserResponse.Id);
        result.Name.ShouldBe(randomUserResponse.Name);
        result.Email.ShouldBe(randomUserResponse.Email);
    }

    [Fact]
    public async Task Send_WithBuilderUsingRandomData_ShouldWorkCorrectly()
    {
        // Arrange
        var handler = HandlerMocks.CreateUserCommandHandler();
        var serviceProvider = ServiceProviderMocks.CreateWithHandler<CreateUserCommand, UserResponse>(handler);
        var nexus = new Nexus(serviceProvider);
        
        // Use builder with random data (builders now use Bogus internally)
        var request = new CreateUserCommandBuilder()
            .WithName(FakeData.GenerateRandomName())
            .WithEmail(FakeData.GenerateRandomEmail())
            .Build();

        // Act
        var result = await nexus.Send(request);

        // Assert
        result.ShouldNotBeNull();
        result.Name.ShouldNotBeNullOrEmpty();
        result.Email.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task Send_MultipleRandomRequests_ShouldHandleAllCorrectly()
    {
        // Arrange
        var handler = new Mock<IRequestHandler<CreateUserCommand, UserResponse>>();
        handler.Setup(x => x.Handle(It.IsAny<CreateUserCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => FakeData.GenerateUserResponse());
        var serviceProvider = ServiceProviderMocks.CreateWithHandler<CreateUserCommand, UserResponse>(handler.Object);
        var nexus = new Nexus(serviceProvider);
        
        // Generate multiple random requests
        var requests = Enumerable.Range(0, 5)
            .Select(_ => FakeData.GenerateCreateUserCommand())
            .ToArray();

        // Act
        var tasks = requests.Select(request => nexus.Send(request));
        var results = await Task.WhenAll(tasks);

        // Assert
        results.Length.ShouldBe(5);
        results.ShouldAllBe(r => r != null);
        
        // Verify that each request had different data
        var uniqueNames = results.Select(r => r.Name).Distinct().Count();
        uniqueNames.ShouldBeGreaterThan(1); // At least some should be different
    }

    [Fact]
    public async Task Publish_WithRandomNotifications_ShouldHandleCorrectly()
    {
        // Arrange
        var handlers = NotificationHandlerMocks.MultipleUserCreatedNotificationHandlers(3);
        var serviceProvider = ServiceProviderMocks.CreateWithNotificationHandlers(handlers);
        var nexus = new Nexus(serviceProvider);
        
        // Generate random notifications
        var notifications = Enumerable.Range(0, 3)
            .Select(_ => FakeData.GenerateUserCreatedNotification())
            .ToArray();

        // Act
        var tasks = notifications.Select(notification => nexus.Publish(notification));
        await Task.WhenAll(tasks);

        // Assert - Should not throw any exceptions
        // The test passes if no exceptions are thrown
    }

    [Fact]
    public async Task Send_WithRandomUserList_ShouldHandleCorrectly()
    {
        // Arrange
        var randomUserList = FakeData.GenerateUserListResponse(10);
        var handler = new Mock<IRequestHandler<GetUserListQuery, UserListResponse>>();
        handler.Setup(x => x.Handle(It.IsAny<GetUserListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(randomUserList);
        
        var serviceProvider = ServiceProviderMocks.CreateWithHandler<GetUserListQuery, UserListResponse>(handler.Object);
        var nexus = new Nexus(serviceProvider);
        var request = FakeData.GenerateGetUserListQuery();

        // Act
        var result = await nexus.Send(request);

        // Assert
        result.ShouldNotBeNull();
        result.Users.Count.ShouldBe(10);
        result.TotalCount.ShouldBe(10);
        result.Users.ShouldAllBe(u => !string.IsNullOrEmpty(u.Name));
        result.Users.ShouldAllBe(u => !string.IsNullOrEmpty(u.Email));
    }

    [Fact]
    public async Task Send_WithRandomDates_ShouldHandleCorrectly()
    {
        // Arrange
        var pastDate = FakeData.GenerateRandomPastDate(1);
        var futureDate = FakeData.GenerateRandomFutureDate(1);
        
        var userResponse = new UserResponseBuilder()
            .WithCreatedAt(pastDate)
            .Build();
            
        var handler = HandlerMocks.CreateUserCommandHandler(userResponse);
        var serviceProvider = ServiceProviderMocks.CreateWithHandler<CreateUserCommand, UserResponse>(handler);
        var nexus = new Nexus(serviceProvider);
        var request = FakeData.GenerateCreateUserCommand();

        // Act
        var result = await nexus.Send(request);

        // Assert
        result.ShouldNotBeNull();
        result.CreatedAt.ShouldBe(pastDate);
        result.CreatedAt.ShouldBeLessThan(DateTime.Now);
    }

    [Fact]
    public async Task Send_WithRandomStrings_ShouldHandleCorrectly()
    {
        // Arrange
        var randomAction = FakeData.GenerateRandomString(15);
        var request = new LogUserActionCommandBuilder()
            .WithAction(randomAction)
            .Build();
            
        var handler = HandlerMocks.LogUserActionCommandHandler();
        var serviceProvider = ServiceProviderMocks.CreateWithHandler<LogUserActionCommand>(handler);
        var nexus = new Nexus(serviceProvider);

        // Act
        var action = async () => await nexus.Send(request);

        // Assert
        await action.ShouldNotThrowAsync();
    }

    [Fact]
    public async Task Send_WithRandomGuids_ShouldHandleCorrectly()
    {
        // Arrange
        var randomUserId = FakeData.GenerateRandomGuid();
        var request = new DeleteUserCommandBuilder()
            .WithUserId(randomUserId)
            .Build();
            
        var handler = HandlerMocks.DeleteUserCommandHandler();
        var serviceProvider = ServiceProviderMocks.CreateWithHandler<DeleteUserCommand>(handler);
        var nexus = new Nexus(serviceProvider);

        // Act
        var action = async () => await nexus.Send(request);

        // Assert
        await action.ShouldNotThrowAsync();
    }
} 