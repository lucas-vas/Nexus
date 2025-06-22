using Shouldly;
using Microsoft.Extensions.DependencyInjection;
using Nexus;
using Nexus.Interfaces;
using Nexus.Tests.Builders;
using Nexus.Tests.Mocks;
using Nexus.Tests.TestModels;
using Xunit;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Nexus.Tests;

/// <summary>
/// Tests for the Nexus mediator class.
/// </summary>
public class NexusTests
{
    [Fact]
    public void Constructor_WithValidServiceProvider_ShouldCreateInstance()
    {
        // Arrange
        var serviceProvider = ServiceProviderMocks.CreateWithHandler<CreateUserCommand, UserResponse>(
            HandlerMocks.CreateUserCommandHandler());

        // Act
        var nexus = new Nexus(serviceProvider);

        // Assert
        nexus.ShouldNotBeNull();
    }

    [Fact]
    public void Constructor_WithNullServiceProvider_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var action = () => new Nexus(null!);
        action.ShouldThrow<ArgumentNullException>()
            .ParamName.ShouldBe("serviceProvider");
    }

    [Fact]
    public async Task Send_WithValidRequest_ShouldReturnResponse()
    {
        // Arrange
        var expectedResponse = new UserResponseBuilder().Build();
        var handler = HandlerMocks.CreateUserCommandHandler(expectedResponse);
        var serviceProvider = ServiceProviderMocks.CreateWithHandler<CreateUserCommand, UserResponse>(handler);
        var nexus = new Nexus(serviceProvider);
        var request = new CreateUserCommandBuilder().Build();

        // Act
        var result = await nexus.Send(request);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task Send_WithNullRequest_ShouldThrowArgumentNullException()
    {
        // Arrange
        var serviceProvider = ServiceProviderMocks.CreateWithHandler<CreateUserCommand, UserResponse>(
            HandlerMocks.CreateUserCommandHandler());
        var nexus = new Nexus(serviceProvider);

        // Act & Assert
        var action = async () => await nexus.Send<UserResponse>(null!);
        var exception = await action.ShouldThrowAsync<ArgumentNullException>();
        exception.ParamName.ShouldBe("request");
    }

    [Fact]
    public async Task Send_WithMissingHandler_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var serviceProvider = ServiceProviderMocks.CreateWithMissingHandler<CreateUserCommand, UserResponse>();
        var nexus = new Nexus(serviceProvider);
        var request = new CreateUserCommandBuilder().Build();

        // Act & Assert
        var action = async () => await nexus.Send(request);
        var exception = await action.ShouldThrowAsync<InvalidOperationException>();
        exception.Message.ShouldContain("No handler registered for request type");
    }

    [Fact]
    public async Task Send_WithHandlerThrowingException_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var handler = new Mock<IRequestHandler<CreateUserCommand, UserResponse>>();
        handler.Setup(x => x.Handle(It.IsAny<CreateUserCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Handler error"));
        var serviceProvider = ServiceProviderMocks.CreateWithHandler<CreateUserCommand, UserResponse>(handler.Object);
        var nexus = new Nexus(serviceProvider);
        var request = new CreateUserCommandBuilder().Build();

        // Act
        var action = async () => await nexus.Send(request);
        var thrownException = await action.ShouldThrowAsync<InvalidOperationException>();
        thrownException.Message.ShouldContain("Handler error");
    }

    [Fact]
    public async Task Send_WithRequestWithoutResponse_ShouldCompleteSuccessfully()
    {
        // Arrange
        var handler = HandlerMocks.DeleteUserCommandHandler();
        var serviceProvider = ServiceProviderMocks.CreateWithHandler<DeleteUserCommand>(handler);
        var nexus = new Nexus(serviceProvider);
        var request = new DeleteUserCommandBuilder().Build();

        // Act
        var action = async () => await nexus.Send(request);

        // Assert
        await action.ShouldNotThrowAsync();
    }

    [Fact]
    public async Task Send_WithRequestWithoutResponseAndNullRequest_ShouldThrowArgumentNullException()
    {
        // Arrange
        var serviceProvider = ServiceProviderMocks.CreateWithHandler<DeleteUserCommand>(
            HandlerMocks.DeleteUserCommandHandler());
        var nexus = new Nexus(serviceProvider);

        // Act & Assert
        var action = async () => await nexus.Send((IRequest)null!);
        var exception = await action.ShouldThrowAsync<ArgumentNullException>();
        exception.ParamName.ShouldBe("request");
    }

    [Fact]
    public async Task Send_WithRequestWithoutResponseAndMissingHandler_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var serviceProvider = ServiceProviderMocks.CreateWithMissingHandler<DeleteUserCommand>();
        var nexus = new Nexus(serviceProvider);
        var request = new DeleteUserCommandBuilder().Build();

        // Act & Assert
        var action = async () => await nexus.Send(request);
        var exception = await action.ShouldThrowAsync<InvalidOperationException>();
        exception.Message.ShouldContain("No handler registered for request type");
    }

    [Fact]
    public async Task Send_WithRequestWithoutResponseAndHandlerThrowingException_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var handler = new Mock<IRequestHandler<DeleteUserCommand>>();
        handler.Setup(x => x.Handle(It.IsAny<DeleteUserCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Handler error"));
        var serviceProvider = ServiceProviderMocks.CreateWithHandler<DeleteUserCommand>(handler.Object);
        var nexus = new Nexus(serviceProvider);
        var request = new DeleteUserCommandBuilder().Build();

        // Act
        var action = async () => await nexus.Send(request);
        var thrownException = await action.ShouldThrowAsync<InvalidOperationException>();
        thrownException.Message.ShouldContain("Handler error");
    }

    [Fact]
    public async Task Publish_WithValidNotification_ShouldCompleteSuccessfully()
    {
        // Arrange
        var handlers = NotificationHandlerMocks.MultipleUserCreatedNotificationHandlers(2);
        var serviceProvider = ServiceProviderMocks.CreateWithNotificationHandlers(handlers);
        var nexus = new Nexus(serviceProvider);
        var notification = new UserCreatedNotification
        {
            UserId = TestData.Users.ValidUserId,
            Email = TestData.Users.ValidEmail
        };

        // Act
        var action = async () => await nexus.Publish(notification);

        // Assert
        await action.ShouldNotThrowAsync();
    }

    [Fact]
    public async Task Publish_WithNullNotification_ShouldThrowArgumentNullException()
    {
        // Arrange
        var serviceProvider = ServiceProviderMocks.CreateWithNotificationHandlers<UserCreatedNotification>(
            NotificationHandlerMocks.MultipleUserCreatedNotificationHandlers());
        var nexus = new Nexus(serviceProvider);

        // Act & Assert
        var action = async () => await nexus.Publish<UserCreatedNotification>(null!);
        var exception = await action.ShouldThrowAsync<ArgumentNullException>();
        exception.ParamName.ShouldBe("notification");
    }

    [Fact]
    public async Task Publish_WithNoHandlers_ShouldCompleteSuccessfully()
    {
        // Arrange
        var serviceProvider = ServiceProviderMocks.CreateWithNoNotificationHandlers<UserCreatedNotification>();
        var nexus = new Nexus(serviceProvider);
        var notification = new UserCreatedNotification
        {
            UserId = TestData.Users.ValidUserId,
            Email = TestData.Users.ValidEmail
        };

        // Act
        var action = async () => await nexus.Publish(notification);

        // Assert
        await action.ShouldNotThrowAsync();
    }

    [Fact]
    public async Task Publish_WithHandlerThrowingException_ShouldThrowException()
    {
        // Arrange
        var handler = new Mock<INotificationHandler<UserCreatedNotification>>();
        handler.Setup(x => x.Handle(It.IsAny<UserCreatedNotification>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Handler error"));
        var serviceProvider = ServiceProviderMocks.CreateWithNotificationHandlers(new[] { handler.Object });
        var nexus = new Nexus(serviceProvider);
        var notification = new UserCreatedNotification { UserId = Guid.NewGuid(), Email = "test@example.com" };

        // Act
        var action = async () => await nexus.Publish(notification);
        var ex = await action.ShouldThrowAsync<Exception>();
        ex.Message.ShouldBe("Handler error");
    }

    [Fact]
    public async Task Send_WithCancellationToken_ShouldRespectCancellation()
    {
        // Arrange
        var handler = new Mock<IRequestHandler<CreateUserCommand, UserResponse>>();
        handler.Setup(x => x.Handle(It.IsAny<CreateUserCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TaskCanceledException());
        var serviceProvider = ServiceProviderMocks.CreateWithHandler<CreateUserCommand, UserResponse>(handler.Object);
        var nexus = new Nexus(serviceProvider);
        var request = new CreateUserCommandBuilder().Build();
        var cts = new CancellationTokenSource(100);

        // Act
        var action = async () => await nexus.Send(request, cts.Token);
        await action.ShouldThrowAsync<TaskCanceledException>();
    }

    [Fact]
    public async Task Publish_WithCancellationToken_ShouldRespectCancellation()
    {
        // Arrange
        var handler = new Mock<INotificationHandler<UserCreatedNotification>>();
        handler.Setup(x => x.Handle(It.IsAny<UserCreatedNotification>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());
        var serviceProvider = ServiceProviderMocks.CreateWithNotificationHandlers(new[] { handler.Object });
        var nexus = new Nexus(serviceProvider);
        var notification = new UserCreatedNotification { UserId = Guid.NewGuid(), Email = "test@example.com" };
        var cts = new CancellationTokenSource(100);

        // Act
        var action = async () => await nexus.Publish(notification, cts.Token);
        await action.ShouldThrowAsync<OperationCanceledException>();
    }
} 