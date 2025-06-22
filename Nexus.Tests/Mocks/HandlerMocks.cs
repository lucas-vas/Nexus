namespace Nexus.Tests.Mocks;

/// <summary>
/// Mock setups for request handlers.
/// </summary>
public static class HandlerMocks
{
    /// <summary>
    /// Creates a mock for CreateUserCommandHandler.
    /// </summary>
    /// <param name="response">The response to return.</param>
    /// <returns>The mock handler.</returns>
    public static IRequestHandler<CreateUserCommand, UserResponse> CreateUserCommandHandler(UserResponse? response = null)
    {
        var mock = new Mock<IRequestHandler<CreateUserCommand, UserResponse>>();
        var defaultResponse = response ?? FakeData.GenerateUserResponse();
        
        mock.Setup(x => x.Handle(It.IsAny<CreateUserCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(defaultResponse);
        
        return mock.Object;
    }

    /// <summary>
    /// Creates a mock for CreateUserCommandHandler that throws an exception.
    /// </summary>
    /// <param name="exception">The exception to throw.</param>
    /// <returns>The mock handler.</returns>
    public static IRequestHandler<CreateUserCommand, UserResponse> CreateUserCommandHandlerWithException(Exception exception)
    {
        var mock = new Mock<IRequestHandler<CreateUserCommand, UserResponse>>();
        
        mock.Setup(x => x.Handle(It.IsAny<CreateUserCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);
        
        return mock.Object;
    }

    /// <summary>
    /// Creates a mock for GetUserQueryHandler.
    /// </summary>
    /// <param name="response">The response to return.</param>
    /// <returns>The mock handler.</returns>
    public static IRequestHandler<GetUserQuery, UserResponse> GetUserQueryHandler(UserResponse? response = null)
    {
        var mock = new Mock<IRequestHandler<GetUserQuery, UserResponse>>();
        var defaultResponse = response ?? FakeData.GenerateUserResponse();
        
        mock.Setup(x => x.Handle(It.IsAny<GetUserQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(defaultResponse);
        
        return mock.Object;
    }

    /// <summary>
    /// Creates a mock for UpdateUserCommandHandler.
    /// </summary>
    /// <param name="response">The response to return.</param>
    /// <returns>The mock handler.</returns>
    public static IRequestHandler<UpdateUserCommand, UserResponse> UpdateUserCommandHandler(UserResponse? response = null)
    {
        var mock = new Mock<IRequestHandler<UpdateUserCommand, UserResponse>>();
        var defaultResponse = response ?? FakeData.GenerateUserResponse();
        
        mock.Setup(x => x.Handle(It.IsAny<UpdateUserCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(defaultResponse);
        
        return mock.Object;
    }

    /// <summary>
    /// Creates a mock for DeleteUserCommandHandler.
    /// </summary>
    /// <returns>The mock handler.</returns>
    public static IRequestHandler<DeleteUserCommand> DeleteUserCommandHandler()
    {
        var mock = new Mock<IRequestHandler<DeleteUserCommand>>();
        
        mock.Setup(x => x.Handle(It.IsAny<DeleteUserCommand>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        
        return mock.Object;
    }

    /// <summary>
    /// Creates a mock for DeleteUserCommandHandler that throws an exception.
    /// </summary>
    /// <param name="exception">The exception to throw.</param>
    /// <returns>The mock handler.</returns>
    public static IRequestHandler<DeleteUserCommand> DeleteUserCommandHandlerWithException(Exception exception)
    {
        var mock = new Mock<IRequestHandler<DeleteUserCommand>>();
        
        mock.Setup(x => x.Handle(It.IsAny<DeleteUserCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);
        
        return mock.Object;
    }

    /// <summary>
    /// Creates a mock for LogUserActionCommandHandler.
    /// </summary>
    /// <returns>The mock handler.</returns>
    public static IRequestHandler<LogUserActionCommand> LogUserActionCommandHandler()
    {
        var mock = new Mock<IRequestHandler<LogUserActionCommand>>();
        
        mock.Setup(x => x.Handle(It.IsAny<LogUserActionCommand>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        
        return mock.Object;
    }
} 