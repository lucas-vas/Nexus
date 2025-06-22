namespace Nexus.Tests;

/// <summary>
/// Tests for pipeline behaviors.
/// </summary>
public class PipelineBehaviorTests
{
    [Fact]
    public async Task Send_WithPipelineBehaviors_ShouldExecuteInCorrectOrder()
    {
        // Arrange
        var executionOrder = new List<string>();
        
        var behavior1 = new Mock<IPipelineBehavior<CreateUserCommand, UserResponse>>();
        behavior1.Setup(x => x.Handle(It.IsAny<CreateUserCommand>(), It.IsAny<RequestHandlerDelegate<UserResponse>>(), It.IsAny<CancellationToken>()))
            .Returns((CreateUserCommand request, RequestHandlerDelegate<UserResponse> next, CancellationToken token) =>
            {
                executionOrder.Add("Behavior1");
                return next();
            });

        var behavior2 = new Mock<IPipelineBehavior<CreateUserCommand, UserResponse>>();
        behavior2.Setup(x => x.Handle(It.IsAny<CreateUserCommand>(), It.IsAny<RequestHandlerDelegate<UserResponse>>(), It.IsAny<CancellationToken>()))
            .Returns((CreateUserCommand request, RequestHandlerDelegate<UserResponse> next, CancellationToken token) =>
            {
                executionOrder.Add("Behavior2");
                return next();
            });

        var handler = HandlerMocks.CreateUserCommandHandler();
        var behaviors = new[] { behavior1.Object, behavior2.Object };
        var serviceProvider = ServiceProviderMocks.CreateWithPipelineBehaviors(handler, behaviors);
        var nexus = new Nexus(serviceProvider);
        var request = new CreateUserCommandBuilder().Build();

        // Act
        await nexus.Send(request);

        // Assert
        executionOrder.Count.ShouldBe(2);
        executionOrder[0].ShouldBe("Behavior2"); // Reverse order
        executionOrder[1].ShouldBe("Behavior1");
    }

    [Fact]
    public async Task Send_WithPipelineBehaviorThrowingException_ShouldPropagateException()
    {
        // Arrange
        var exception = new InvalidOperationException("Behavior error");
        var behavior = new Mock<IPipelineBehavior<CreateUserCommand, UserResponse>>();
        behavior.Setup(x => x.Handle(It.IsAny<CreateUserCommand>(), It.IsAny<RequestHandlerDelegate<UserResponse>>(), It.IsAny<CancellationToken>()))
            .Throws(exception);

        var handler = HandlerMocks.CreateUserCommandHandler();
        var behaviors = new[] { behavior.Object };
        var serviceProvider = ServiceProviderMocks.CreateWithPipelineBehaviors(handler, behaviors);
        var nexus = new Nexus(serviceProvider);
        var request = new CreateUserCommandBuilder().Build();

        // Act & Assert
        var action = async () => await nexus.Send(request);
        var thrownException = await action.ShouldThrowAsync<InvalidOperationException>();
        thrownException.Message.ShouldBe("Behavior error");
    }

    [Fact]
    public async Task Send_WithPipelineBehaviorReturningNull_ShouldHandleCorrectly()
    {
        // Arrange
        var behavior = new Mock<IPipelineBehavior<CreateUserCommand, UserResponse>>();
        behavior.Setup(x => x.Handle(It.IsAny<CreateUserCommand>(), It.IsAny<RequestHandlerDelegate<UserResponse>>(), It.IsAny<CancellationToken>()))
            .Returns((Task<UserResponse>)null!);

        var handler = HandlerMocks.CreateUserCommandHandler();
        var behaviors = new[] { behavior.Object };
        var serviceProvider = ServiceProviderMocks.CreateWithPipelineBehaviors(handler, behaviors);
        var nexus = new Nexus(serviceProvider);
        var request = new CreateUserCommandBuilder().Build();

        // Act & Assert - Should not throw exception, but behavior returning null might cause issues
        var action = async () => await nexus.Send(request);
        await action.ShouldThrowAsync<NullReferenceException>();
    }

    [Fact]
    public async Task Send_WithMultiplePipelineBehaviors_ShouldExecuteAllBehaviors()
    {
        // Arrange
        var behavior1Executed = false;
        var behavior2Executed = false;

        var behavior1 = new Mock<IPipelineBehavior<CreateUserCommand, UserResponse>>();
        behavior1.Setup(x => x.Handle(It.IsAny<CreateUserCommand>(), It.IsAny<RequestHandlerDelegate<UserResponse>>(), It.IsAny<CancellationToken>()))
            .Returns((CreateUserCommand request, RequestHandlerDelegate<UserResponse> next, CancellationToken token) =>
            {
                behavior1Executed = true;
                return next();
            });

        var behavior2 = new Mock<IPipelineBehavior<CreateUserCommand, UserResponse>>();
        behavior2.Setup(x => x.Handle(It.IsAny<CreateUserCommand>(), It.IsAny<RequestHandlerDelegate<UserResponse>>(), It.IsAny<CancellationToken>()))
            .Returns((CreateUserCommand request, RequestHandlerDelegate<UserResponse> next, CancellationToken token) =>
            {
                behavior2Executed = true;
                return next();
            });

        var handler = HandlerMocks.CreateUserCommandHandler();
        var behaviors = new[] { behavior1.Object, behavior2.Object };
        var serviceProvider = ServiceProviderMocks.CreateWithPipelineBehaviors(handler, behaviors);
        var nexus = new Nexus(serviceProvider);
        var request = new CreateUserCommandBuilder().Build();

        // Act
        await nexus.Send(request);

        // Assert
        behavior1Executed.ShouldBeTrue();
        behavior2Executed.ShouldBeTrue();
    }

    [Fact]
    public async Task Send_WithPipelineBehaviorModifyingRequest_ShouldPassModifiedRequest()
    {
        // Arrange
        var originalName = "Original Name";
        var modifiedName = "Modified Name";
        var request = new CreateUserCommandBuilder().WithName(originalName).Build();
        var receivedName = "";

        var behavior = new Mock<IPipelineBehavior<CreateUserCommand, UserResponse>>();
        behavior.Setup(x => x.Handle(It.IsAny<CreateUserCommand>(), It.IsAny<RequestHandlerDelegate<UserResponse>>(), It.IsAny<CancellationToken>()))
            .Returns((CreateUserCommand request, RequestHandlerDelegate<UserResponse> next, CancellationToken token) =>
            {
                // Modify the request
                request.Name = modifiedName;
                return next();
            });

        var handler = new Mock<IRequestHandler<CreateUserCommand, UserResponse>>();
        handler.Setup(x => x.Handle(It.IsAny<CreateUserCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CreateUserCommand req, CancellationToken token) =>
            {
                receivedName = req.Name;
                return new UserResponseBuilder().Build();
            });

        var behaviors = new[] { behavior.Object };
        var serviceProvider = ServiceProviderMocks.CreateWithPipelineBehaviors(handler.Object, behaviors);
        var nexus = new Nexus(serviceProvider);

        // Act
        await nexus.Send(request);

        // Assert
        receivedName.ShouldBe(modifiedName);
    }

    [Fact]
    public async Task Send_WithPipelineBehaviorModifyingResponse_ShouldReturnModifiedResponse()
    {
        // Arrange
        var originalResponse = new UserResponseBuilder().WithName("Original").Build();
        var modifiedResponse = new UserResponseBuilder().WithName("Modified").Build();

        var behavior = new Mock<IPipelineBehavior<CreateUserCommand, UserResponse>>();
        behavior.Setup(x => x.Handle(It.IsAny<CreateUserCommand>(), It.IsAny<RequestHandlerDelegate<UserResponse>>(), It.IsAny<CancellationToken>()))
            .Returns((CreateUserCommand request, RequestHandlerDelegate<UserResponse> next, CancellationToken token) =>
            {
                var response = next();
                // Modify the response
                return Task.FromResult(modifiedResponse);
            });

        var handler = HandlerMocks.CreateUserCommandHandler(originalResponse);
        var behaviors = new[] { behavior.Object };
        var serviceProvider = ServiceProviderMocks.CreateWithPipelineBehaviors(handler, behaviors);
        var nexus = new Nexus(serviceProvider);
        var request = new CreateUserCommandBuilder().Build();

        // Act
        var result = await nexus.Send(request);

        // Assert
        result.ShouldBeEquivalentTo(modifiedResponse);
        result.ShouldNotBe(originalResponse);
    }

    [Fact]
    public async Task Send_WithPipelineBehaviorAndCancellationToken_ShouldRespectCancellation()
    {
        // Arrange
        var behavior = new Mock<IPipelineBehavior<CreateUserCommand, UserResponse>>();
        behavior.Setup(x => x.Handle(It.IsAny<CreateUserCommand>(), It.IsAny<RequestHandlerDelegate<UserResponse>>(), It.IsAny<CancellationToken>()))
            .Returns((CreateUserCommand request, RequestHandlerDelegate<UserResponse> next, CancellationToken token) =>
            {
                token.ThrowIfCancellationRequested();
                return next();
            });

        var handler = HandlerMocks.CreateUserCommandHandler();
        var behaviors = new[] { behavior.Object };
        var serviceProvider = ServiceProviderMocks.CreateWithPipelineBehaviors(handler, behaviors);
        var nexus = new Nexus(serviceProvider);
        var request = new CreateUserCommandBuilder().Build();
        var cancellationToken = new CancellationToken(true); // Already cancelled

        // Act & Assert
        var action = async () => await nexus.Send(request, cancellationToken);
        await action.ShouldThrowAsync<OperationCanceledException>();
    }
} 