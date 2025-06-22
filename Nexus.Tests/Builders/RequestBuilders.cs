using Nexus.Tests.Common;
using Nexus.Tests.TestModels;

namespace Nexus.Tests.Builders;

/// <summary>
/// Builder for CreateUserCommand objects.
/// </summary>
public class CreateUserCommandBuilder : BuilderBase<CreateUserCommand>
{
    private string _name = FakeData.GenerateRandomName();
    private string _email = FakeData.GenerateRandomEmail();

    /// <summary>
    /// Sets the user name.
    /// </summary>
    /// <param name="name">The user name.</param>
    /// <returns>The builder instance.</returns>
    public CreateUserCommandBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    /// <summary>
    /// Sets the user email.
    /// </summary>
    /// <param name="email">The user email.</param>
    /// <returns>The builder instance.</returns>
    public CreateUserCommandBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }

    /// <summary>
    /// Builds the CreateUserCommand object.
    /// </summary>
    /// <returns>The built CreateUserCommand.</returns>
    public override CreateUserCommand Build()
    {
        return new CreateUserCommand
        {
            Name = _name,
            Email = _email
        };
    }
}

/// <summary>
/// Builder for GetUserQuery objects.
/// </summary>
public class GetUserQueryBuilder : BuilderBase<GetUserQuery>
{
    private Guid _userId = FakeData.GenerateRandomGuid();

    /// <summary>
    /// Sets the user ID.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <returns>The builder instance.</returns>
    public GetUserQueryBuilder WithUserId(Guid userId)
    {
        _userId = userId;
        return this;
    }

    /// <summary>
    /// Builds the GetUserQuery object.
    /// </summary>
    /// <returns>The built GetUserQuery.</returns>
    public override GetUserQuery Build()
    {
        return new GetUserQuery
        {
            UserId = _userId
        };
    }
}

/// <summary>
/// Builder for GetUserListQuery objects.
/// </summary>
public class GetUserListQueryBuilder : BuilderBase<GetUserListQuery>
{
    private int _page = 1;
    private int _pageSize = 10;
    private string? _searchTerm = null;

    /// <summary>
    /// Sets the page number.
    /// </summary>
    /// <param name="page">The page number.</param>
    /// <returns>The builder instance.</returns>
    public GetUserListQueryBuilder WithPage(int page)
    {
        _page = page;
        return this;
    }

    /// <summary>
    /// Sets the page size.
    /// </summary>
    /// <param name="pageSize">The page size.</param>
    /// <returns>The builder instance.</returns>
    public GetUserListQueryBuilder WithPageSize(int pageSize)
    {
        _pageSize = pageSize;
        return this;
    }

    /// <summary>
    /// Sets the search term.
    /// </summary>
    /// <param name="searchTerm">The search term.</param>
    /// <returns>The builder instance.</returns>
    public GetUserListQueryBuilder WithSearchTerm(string? searchTerm)
    {
        _searchTerm = searchTerm;
        return this;
    }

    /// <summary>
    /// Builds the GetUserListQuery object.
    /// </summary>
    /// <returns>The built GetUserListQuery.</returns>
    public override GetUserListQuery Build()
    {
        return new GetUserListQuery
        {
            Page = _page,
            PageSize = _pageSize,
            SearchTerm = _searchTerm
        };
    }
}

/// <summary>
/// Builder for UpdateUserCommand objects.
/// </summary>
public class UpdateUserCommandBuilder : BuilderBase<UpdateUserCommand>
{
    private Guid _userId = FakeData.GenerateRandomGuid();
    private string _name = FakeData.GenerateRandomName();
    private string _email = FakeData.GenerateRandomEmail();

    /// <summary>
    /// Sets the user ID.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <returns>The builder instance.</returns>
    public UpdateUserCommandBuilder WithUserId(Guid userId)
    {
        _userId = userId;
        return this;
    }

    /// <summary>
    /// Sets the user name.
    /// </summary>
    /// <param name="name">The user name.</param>
    /// <returns>The builder instance.</returns>
    public UpdateUserCommandBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    /// <summary>
    /// Sets the user email.
    /// </summary>
    /// <param name="email">The user email.</param>
    /// <returns>The builder instance.</returns>
    public UpdateUserCommandBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }

    /// <summary>
    /// Builds the UpdateUserCommand object.
    /// </summary>
    /// <returns>The built UpdateUserCommand.</returns>
    public override UpdateUserCommand Build()
    {
        return new UpdateUserCommand
        {
            UserId = _userId,
            Name = _name,
            Email = _email
        };
    }
}

/// <summary>
/// Builder for DeleteUserCommand objects.
/// </summary>
public class DeleteUserCommandBuilder : BuilderBase<DeleteUserCommand>
{
    private Guid _userId = FakeData.GenerateRandomGuid();

    /// <summary>
    /// Sets the user ID.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <returns>The builder instance.</returns>
    public DeleteUserCommandBuilder WithUserId(Guid userId)
    {
        _userId = userId;
        return this;
    }

    /// <summary>
    /// Builds the DeleteUserCommand object.
    /// </summary>
    /// <returns>The built DeleteUserCommand.</returns>
    public override DeleteUserCommand Build()
    {
        return new DeleteUserCommand
        {
            UserId = _userId
        };
    }
}

/// <summary>
/// Builder for LogUserActionCommand objects.
/// </summary>
public class LogUserActionCommandBuilder : BuilderBase<LogUserActionCommand>
{
    private Guid _userId = FakeData.GenerateRandomGuid();
    private string _action = FakeData.GenerateRandomString(10);

    /// <summary>
    /// Sets the user ID.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <returns>The builder instance.</returns>
    public LogUserActionCommandBuilder WithUserId(Guid userId)
    {
        _userId = userId;
        return this;
    }

    /// <summary>
    /// Sets the action.
    /// </summary>
    /// <param name="action">The action.</param>
    /// <returns>The builder instance.</returns>
    public LogUserActionCommandBuilder WithAction(string action)
    {
        _action = action;
        return this;
    }

    /// <summary>
    /// Builds the LogUserActionCommand object.
    /// </summary>
    /// <returns>The built LogUserActionCommand.</returns>
    public override LogUserActionCommand Build()
    {
        return new LogUserActionCommand
        {
            UserId = _userId,
            Action = _action
        };
    }
} 