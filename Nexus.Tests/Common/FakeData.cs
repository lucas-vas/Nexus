using Bogus;
using Nexus.Tests.TestModels;

namespace Nexus.Tests.Common;

/// <summary>
/// Provides fake data generation using Bogus/Faker for testing purposes.
/// </summary>
public static class FakeData
{
    private static readonly Faker _faker = new Faker("en");
    
    /// <summary>
    /// Generates a fake user response with random data.
    /// </summary>
    /// <returns>A UserResponse with random data.</returns>
    public static UserResponse GenerateUserResponse()
    {
        return new Faker<UserResponse>()
            .RuleFor(u => u.Id, f => f.Random.Guid())
            .RuleFor(u => u.Name, f => f.Name.FullName())
            .RuleFor(u => u.Email, (f, u) => f.Internet.Email(u.Name))
            .RuleFor(u => u.CreatedAt, f => f.Date.Past(2))
            .Generate();
    }

    /// <summary>
    /// Generates a fake create user command with random data.
    /// </summary>
    /// <returns>A CreateUserCommand with random data.</returns>
    public static CreateUserCommand GenerateCreateUserCommand()
    {
        return new Faker<CreateUserCommand>()
            .RuleFor(c => c.Name, f => f.Name.FullName())
            .RuleFor(c => c.Email, (f, c) => f.Internet.Email(c.Name))
            .Generate();
    }

    /// <summary>
    /// Generates a fake get user query with random data.
    /// </summary>
    /// <returns>A GetUserQuery with random data.</returns>
    public static GetUserQuery GenerateGetUserQuery()
    {
        return new Faker<GetUserQuery>()
            .RuleFor(q => q.UserId, f => f.Random.Guid())
            .Generate();
    }

    /// <summary>
    /// Generates a fake get user list query with random data.
    /// </summary>
    /// <returns>A GetUserListQuery with random data.</returns>
    public static GetUserListQuery GenerateGetUserListQuery()
    {
        return new Faker<GetUserListQuery>()
            .RuleFor(q => q.Page, f => f.Random.Int(1, 10))
            .RuleFor(q => q.PageSize, f => f.PickRandom(5, 10, 20, 50))
            .RuleFor(q => q.SearchTerm, f => f.Random.Bool() ? f.Name.FirstName() : null)
            .Generate();
    }

    /// <summary>
    /// Generates a fake update user command with random data.
    /// </summary>
    /// <returns>An UpdateUserCommand with random data.</returns>
    public static UpdateUserCommand GenerateUpdateUserCommand()
    {
        return new Faker<UpdateUserCommand>()
            .RuleFor(c => c.UserId, f => f.Random.Guid())
            .RuleFor(c => c.Name, f => f.Name.FullName())
            .RuleFor(c => c.Email, (f, c) => f.Internet.Email(c.Name))
            .Generate();
    }

    /// <summary>
    /// Generates a fake delete user command with random data.
    /// </summary>
    /// <returns>A DeleteUserCommand with random data.</returns>
    public static DeleteUserCommand GenerateDeleteUserCommand()
    {
        return new Faker<DeleteUserCommand>()
            .RuleFor(c => c.UserId, f => f.Random.Guid())
            .Generate();
    }

    /// <summary>
    /// Generates a fake log user action command with random data.
    /// </summary>
    /// <returns>A LogUserActionCommand with random data.</returns>
    public static LogUserActionCommand GenerateLogUserActionCommand()
    {
        return new Faker<LogUserActionCommand>()
            .RuleFor(c => c.UserId, f => f.Random.Guid())
            .RuleFor(c => c.Action, f => f.PickRandom("Login", "Logout", "Update", "Delete", "Create"))
            .Generate();
    }

    /// <summary>
    /// Generates a fake user created notification with random data.
    /// </summary>
    /// <returns>A UserCreatedNotification with random data.</returns>
    public static UserCreatedNotification GenerateUserCreatedNotification()
    {
        return new Faker<UserCreatedNotification>()
            .RuleFor(n => n.UserId, f => f.Random.Guid())
            .RuleFor(n => n.Email, f => f.Internet.Email())
            .Generate();
    }

    /// <summary>
    /// Generates a fake user deleted notification with random data.
    /// </summary>
    /// <returns>A UserDeletedNotification with random data.</returns>
    public static UserDeletedNotification GenerateUserDeletedNotification()
    {
        return new Faker<UserDeletedNotification>()
            .RuleFor(n => n.UserId, f => f.Random.Guid())
            .RuleFor(n => n.DeletedAt, f => f.Date.Recent())
            .Generate();
    }

    /// <summary>
    /// Generates a list of fake user responses.
    /// </summary>
    /// <param name="count">Number of user responses to generate.</param>
    /// <returns>A list of UserResponse with random data.</returns>
    public static List<UserResponse> GenerateUserResponses(int count = 10)
    {
        return new Faker<UserResponse>()
            .RuleFor(u => u.Id, f => f.Random.Guid())
            .RuleFor(u => u.Name, f => f.Name.FullName())
            .RuleFor(u => u.Email, (f, u) => f.Internet.Email(u.Name))
            .RuleFor(u => u.CreatedAt, f => f.Date.Past(2))
            .Generate(count);
    }

    /// <summary>
    /// Generates a fake user list response with random data.
    /// </summary>
    /// <param name="userCount">Number of users to include in the response.</param>
    /// <returns>A UserListResponse with random data.</returns>
    public static UserListResponse GenerateUserListResponse(int userCount = 5)
    {
        return new Faker<UserListResponse>()
            .RuleFor(r => r.Users, f => GenerateUserResponses(userCount))
            .RuleFor(r => r.TotalCount, userCount)
            .Generate();
    }

    /// <summary>
    /// Generates a random string with specified length.
    /// </summary>
    /// <param name="length">Length of the string to generate.</param>
    /// <returns>A random string.</returns>
    public static string GenerateRandomString(int length = 10)
    {
        return _faker.Random.AlphaNumeric(length);
    }

    /// <summary>
    /// Generates a random email address.
    /// </summary>
    /// <returns>A random email address.</returns>
    public static string GenerateRandomEmail()
    {
        return _faker.Internet.Email();
    }

    /// <summary>
    /// Generates a random full name.
    /// </summary>
    /// <returns>A random full name.</returns>
    public static string GenerateRandomName()
    {
        return _faker.Name.FullName();
    }

    /// <summary>
    /// Generates a random GUID.
    /// </summary>
    /// <returns>A random GUID.</returns>
    public static Guid GenerateRandomGuid()
    {
        return _faker.Random.Guid();
    }

    /// <summary>
    /// Generates a random date in the past.
    /// </summary>
    /// <param name="yearsBack">Number of years back from now.</param>
    /// <returns>A random date in the past.</returns>
    public static DateTime GenerateRandomPastDate(int yearsBack = 2)
    {
        return _faker.Date.Past(yearsBack);
    }

    /// <summary>
    /// Generates a random date in the future.
    /// </summary>
    /// <param name="yearsAhead">Number of years ahead from now.</param>
    /// <returns>A random date in the future.</returns>
    public static DateTime GenerateRandomFutureDate(int yearsAhead = 2)
    {
        return _faker.Date.Future(yearsAhead);
    }
} 