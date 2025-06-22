# Nexus.Tests

Test project for the NexusX library, implementing the CQRS pattern

## 🎯 Current Status

- ✅ **64/64 tests passing** - 100% success rate
- ✅ **Multi-target support** - .NET 6, 7, and 8
- ✅ **Clean code** - Optimized imports and dependencies
- ✅ **Comprehensive coverage** - All scenarios tested

## Project Structure

### 📁 Common
- **BuilderBase.cs**: Base class for all project builders
- **TestData.cs**: Common reusable test data
- **FakeData.cs**: Bogus/Faker integration for generating random test data
- **TestServiceProvider.cs**: Custom service provider for testing
- **TestServiceProviderHelpers.cs**: Helper methods for service provider setup

### 📁 Builders
- **RequestBuilders.cs**: Builders for test requests
- **ResponseBuilders.cs**: Builders for test responses

### 📁 Mocks
- **HandlerMocks.cs**: Mocks for request handlers
- **NotificationHandlerMocks.cs**: Mocks for notification handlers
- **ServiceProviderMocks.cs**: Mocks for IServiceProvider

### 📁 TestModels
- **TestRequests.cs**: Test classes for requests, responses and notifications

### 📄 Main Tests
- **NexusTests.cs**: Tests for the main Nexus class
- **DependencyInjectionTests.cs**: Tests for DI extension methods
- **PipelineBehaviorTests.cs**: Tests for pipeline behaviors
- **IntegrationTests.cs**: End-to-end integration tests
- **EdgeCaseTests.cs**: Tests for extreme scenarios and edge cases
- **BogusExampleTests.cs**: Examples demonstrating Bogus usage

## How to Run Tests

### Via Visual Studio
1. Open the `Nexus.sln` solution
2. Right-click on the `Nexus.Tests` project
3. Select "Run Tests"

### Via Command Line
```bash
# Run all tests
dotnet test

# Run tests with details
dotnet test --verbosity normal

# Run specific tests
dotnet test --filter "FullyQualifiedName~NexusTests"

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"
```

## Test Results

### ✅ All Tests Passing (64/64)

**Test Categories:**
- **NexusTests**: 15 tests - Core functionality testing
- **DependencyInjectionTests**: 13 tests - DI registration and configuration
- **PipelineBehaviorTests**: 7 tests - Pipeline behavior functionality
- **IntegrationTests**: 6 tests - End-to-end scenarios
- **EdgeCaseTests**: 15 tests - Edge cases and extreme scenarios
- **BogusExampleTests**: 8 tests - Random data generation examples

**Key Test Scenarios Covered:**
- ✅ Constructor validation
- ✅ Send method with/without response
- ✅ Publish method for notifications
- ✅ Exception handling and propagation
- ✅ CancellationToken support
- ✅ Pipeline behaviors execution
- ✅ Dependency injection registration
- ✅ Concurrent operations
- ✅ Timeout scenarios
- ✅ Random data generation
- ✅ Edge cases and error conditions

## Patterns Used

### Builder Pattern
```csharp
var command = new CreateUserCommandBuilder()
    .WithName("John Doe")
    .WithEmail("john@example.com")
    .Build();
```

### Mock Pattern
```csharp
var handler = HandlerMocks.CreateUserCommandHandler();
var serviceProvider = ServiceProviderMocks.CreateWithHandler(handler);
```

### Arrange-Act-Assert (AAA)
```csharp
[Fact]
public async Task Send_WithValidRequest_ShouldReturnResponse()
{
    // Arrange
    var handler = HandlerMocks.CreateUserCommandHandler();
    var serviceProvider = ServiceProviderMocks.CreateWithHandler(handler);
    var nexus = new Nexus(serviceProvider);
    var request = new CreateUserCommandBuilder().Build();

    // Act
    var result = await nexus.Send(request);

    // Assert
    result.ShouldNotBeNull();
}
```

## Bogus Integration for Random Data

The project uses Bogus to generate realistic random data for testing. This helps ensure tests are more robust and cover various data scenarios.

### Using FakeData Class
```csharp
// Generate random user data
var userResponse = FakeData.GenerateUserResponse();
var createCommand = FakeData.GenerateCreateUserCommand();
var randomEmail = FakeData.GenerateRandomEmail();
var randomName = FakeData.GenerateRandomName();
var randomGuid = FakeData.GenerateRandomGuid();

// Generate multiple items
var userList = FakeData.GenerateUserResponses(10);
var notifications = Enumerable.Range(0, 5)
    .Select(_ => FakeData.GenerateUserCreatedNotification())
    .ToArray();
```

### Builders with Random Data
Builders now use Bogus internally for default values:
```csharp
// Builders automatically use random data
var request = new CreateUserCommandBuilder().Build();

// Or override with specific values
var request = new CreateUserCommandBuilder()
    .WithName("Specific Name")
    .WithEmail("specific@email.com")
    .Build();
```

### Example Test with Random Data
```csharp
[Fact]
public async Task Send_WithRandomUserData_ShouldReturnRandomResponse()
{
    // Arrange
    var randomUserResponse = FakeData.GenerateUserResponse();
    var handler = HandlerMocks.CreateUserCommandHandler(randomUserResponse);
    var serviceProvider = ServiceProviderMocks.CreateWithHandler(handler);
    var nexus = new Nexus(serviceProvider);
    var request = FakeData.GenerateCreateUserCommand();

    // Act
    var result = await nexus.Send(request);

    // Assert
    result.ShouldNotBeNull();
    result.Id.ShouldBe(randomUserResponse.Id);
    result.Name.ShouldBe(randomUserResponse.Name);
    result.Email.ShouldBe(randomUserResponse.Email);
}
```

## Test Coverage

The project includes tests for:

- ✅ Nexus class constructor
- ✅ Send method with response
- ✅ Send method without response
- ✅ Publish method for notifications
- ✅ Exception handling
- ✅ CancellationToken
- ✅ Pipeline behaviors
- ✅ Dependency injection
- ✅ Edge case scenarios
- ✅ Integration tests
- ✅ Concurrency
- ✅ Timeouts
- ✅ Random data generation with Bogus

## Dependencies

- **xUnit**: Testing framework
- **Moq**: Mocking library
- **Shouldly**: More expressive and readable assertions
- **Microsoft.NET.Test.Sdk**: .NET testing SDK
- **Bogus**: Random data generation library

## Configuration

The project is configured for:
- Multi-target: .NET 6, 7 and 8
- Parallel test execution
- Detailed diagnostics
- 30-second timeout for long tests

## Assertions with Shouldly

The project uses Shouldly for more readable assertions:

```csharp
// Basic checks
result.ShouldNotBeNull();
result.ShouldBe(expectedValue);
result.ShouldBeGreaterThan(0);

// Exception checks
await action.ShouldThrowAsync<InvalidOperationException>();
action.ShouldNotThrow();

// Collection checks
results.Length.ShouldBe(10);
results.ShouldAllBe(r => r != null);

// Type checks
nexus.ShouldBeOfType<Nexus>();
```

## Code Quality

### Recent Improvements
- ✅ Removed unused using statements
- ✅ Optimized imports across all files
- ✅ Clean, maintainable test structure
- ✅ Consistent naming conventions
- ✅ Comprehensive error handling tests

### Best Practices
- All tests follow AAA pattern
- Mock objects are properly configured
- Random data generation for robust testing
- Clear test names and descriptions
- Proper exception testing
- Integration tests with real service providers

## Contributing

When adding new tests:

1. Use existing builders or create new ones following the pattern
2. Separate mocks into specific classes
3. Follow the AAA pattern (Arrange-Act-Assert)
4. Use Shouldly for more readable assertions
5. Add tests for error scenarios and edge cases
6. Keep tests independent and isolated
7. Use Bogus for generating random test data when appropriate
8. Add new FakeData methods for new data types 