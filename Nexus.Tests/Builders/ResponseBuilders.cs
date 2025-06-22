using Nexus.Tests.Common;
using Nexus.Tests.TestModels;

namespace Nexus.Tests.Builders;

/// <summary>
/// Builder for UserResponse objects.
/// </summary>
public class UserResponseBuilder : BuilderBase<UserResponse>
{
    private Guid _id = FakeData.GenerateRandomGuid();
    private string _name = FakeData.GenerateRandomName();
    private string _email = FakeData.GenerateRandomEmail();
    private DateTime _createdAt = FakeData.GenerateRandomPastDate();

    /// <summary>
    /// Sets the user ID.
    /// </summary>
    /// <param name="id">The user ID.</param>
    /// <returns>The builder instance.</returns>
    public UserResponseBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    /// <summary>
    /// Sets the user name.
    /// </summary>
    /// <param name="name">The user name.</param>
    /// <returns>The builder instance.</returns>
    public UserResponseBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    /// <summary>
    /// Sets the user email.
    /// </summary>
    /// <param name="email">The user email.</param>
    /// <returns>The builder instance.</returns>
    public UserResponseBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }

    /// <summary>
    /// Sets the creation date.
    /// </summary>
    /// <param name="createdAt">The creation date.</param>
    /// <returns>The builder instance.</returns>
    public UserResponseBuilder WithCreatedAt(DateTime createdAt)
    {
        _createdAt = createdAt;
        return this;
    }

    /// <summary>
    /// Builds the UserResponse object.
    /// </summary>
    /// <returns>The built UserResponse.</returns>
    public override UserResponse Build()
    {
        return new UserResponse
        {
            Id = _id,
            Name = _name,
            Email = _email,
            CreatedAt = _createdAt
        };
    }
}

/// <summary>
/// Builder for UserListResponse objects.
/// </summary>
public class UserListResponseBuilder : BuilderBase<UserListResponse>
{
    private List<UserResponse> _users = FakeData.GenerateUserResponses(5);
    private int _totalCount = 5;

    /// <summary>
    /// Sets the users list.
    /// </summary>
    /// <param name="users">The users list.</param>
    /// <returns>The builder instance.</returns>
    public UserListResponseBuilder WithUsers(List<UserResponse> users)
    {
        _users = users;
        _totalCount = users.Count;
        return this;
    }

    /// <summary>
    /// Sets the total count.
    /// </summary>
    /// <param name="totalCount">The total count.</param>
    /// <returns>The builder instance.</returns>
    public UserListResponseBuilder WithTotalCount(int totalCount)
    {
        _totalCount = totalCount;
        return this;
    }

    /// <summary>
    /// Adds a user to the list.
    /// </summary>
    /// <param name="user">The user to add.</param>
    /// <returns>The builder instance.</returns>
    public UserListResponseBuilder AddUser(UserResponse user)
    {
        _users.Add(user);
        _totalCount++;
        return this;
    }

    /// <summary>
    /// Builds the UserListResponse object.
    /// </summary>
    /// <returns>The built UserListResponse.</returns>
    public override UserListResponse Build()
    {
        return new UserListResponse
        {
            Users = _users,
            TotalCount = _totalCount
        };
    }
} 