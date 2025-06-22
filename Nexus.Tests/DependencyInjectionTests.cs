namespace Nexus.Tests;

/// <summary>
/// Tests for the DependencyInjection extension methods.
/// </summary>
public class DependencyInjectionTests
{
    [Fact]
    public void AddNexus_WithValidAssembly_ShouldRegisterServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var assembly = typeof(CreateUserCommand).Assembly;

        // Act
        services.AddNexus(assembly);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var nexus = serviceProvider.GetService<INexus>();
        nexus.ShouldNotBeNull();
        nexus.ShouldBeOfType<Nexus>();
    }

    [Fact]
    public void AddNexus_WithNullServices_ShouldThrowArgumentNullException()
    {
        // Arrange
        IServiceCollection? services = null;
        var assembly = typeof(CreateUserCommand).Assembly;

        // Act & Assert
        var action = () => services!.AddNexus(assembly);
        var exception = action.ShouldThrow<ArgumentNullException>();
        exception.ParamName.ShouldBe("services");
    }

    [Fact]
    public void AddNexus_WithNullAssembly_ShouldThrowArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();
        Assembly? assembly = null;

        // Act & Assert
        var action = () => services.AddNexus(assembly!);
        var exception = action.ShouldThrow<ArgumentNullException>();
        exception.ParamName.ShouldBe("assembly");
    }

    [Fact]
    public void AddNexus_WithMultipleAssemblies_ShouldRegisterServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var assembly1 = typeof(CreateUserCommand).Assembly;
        var assembly2 = typeof(DependencyInjectionTests).Assembly;

        // Act
        services.AddNexus(assembly1, assembly2);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var nexus = serviceProvider.GetService<INexus>();
        nexus.ShouldNotBeNull();
    }

    [Fact]
    public void AddNexus_WithNullAssemblies_ShouldThrowArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();
        Assembly[]? assemblies = null;

        // Act & Assert
        var action = () => services.AddNexus(assemblies!);
        var exception = action.ShouldThrow<ArgumentNullException>();
        exception.ParamName.ShouldBe("assemblies");
    }

    [Fact]
    public void AddNexus_WithNullAssemblyInArray_ShouldSkipNullAssembly()
    {
        // Arrange
        var services = new ServiceCollection();
        var assembly1 = typeof(CreateUserCommand).Assembly;
        Assembly? assembly2 = null;

        // Act
        var action = () => services.AddNexus(assembly1, assembly2!);

        // Assert
        action.ShouldNotThrow();
    }

    [Fact]
    public void DebugRegisteredHandlers_WithValidServiceProvider_ShouldNotThrow()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddNexus(typeof(CreateUserCommand).Assembly);
        var serviceProvider = services.BuildServiceProvider();

        // Act & Assert
        var action = () => serviceProvider.DebugRegisteredHandlers();
        action.ShouldNotThrow();
    }

    [Fact]
    public void DebugRegisteredHandlers_WithNullServiceProvider_ShouldThrowArgumentNullException()
    {
        // Arrange
        IServiceProvider? serviceProvider = null;

        // Act & Assert
        var action = () => serviceProvider!.DebugRegisteredHandlers();
        var exception = action.ShouldThrow<ArgumentNullException>();
        exception.ParamName.ShouldBe("serviceProvider");
    }

    [Fact]
    public void AddNexus_ShouldRegisterHandlersAsTransient()
    {
        // Arrange
        var services = new ServiceCollection();
        var assembly = typeof(CreateUserCommand).Assembly;

        // Act
        services.AddNexus(assembly);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        
        // Get the same handler twice - they should be different instances (transient)
        var handler1 = serviceProvider.GetService<IRequestHandler<CreateUserCommand, UserResponse>>();
        var handler2 = serviceProvider.GetService<IRequestHandler<CreateUserCommand, UserResponse>>();
        
        handler1.ShouldNotBeSameAs(handler2);
    }

    [Fact]
    public void AddNexus_ShouldRegisterNexusAsScoped()
    {
        // Arrange
        var services = new ServiceCollection();
        var assembly = typeof(CreateUserCommand).Assembly;

        // Act
        services.AddNexus(assembly);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        
        // Create two scopes
        using var scope1 = serviceProvider.CreateScope();
        using var scope2 = serviceProvider.CreateScope();
        
        var nexus1 = scope1.ServiceProvider.GetService<INexus>();
        var nexus2 = scope2.ServiceProvider.GetService<INexus>();
        
        // They should be different instances (scoped)
        nexus1.ShouldNotBeSameAs(nexus2);
    }

    [Fact]
    public void AddNexus_WithAssemblyContainingNoHandlers_ShouldNotThrow()
    {
        // Arrange
        var services = new ServiceCollection();
        var assembly = typeof(DependencyInjectionTests).Assembly; // Assembly without handlers

        // Act & Assert
        var action = () => services.AddNexus(assembly);
        action.ShouldNotThrow();
    }

    [Fact]
    public void AddNexus_ShouldReturnServiceCollectionForChaining()
    {
        // Arrange
        var services = new ServiceCollection();
        var assembly = typeof(CreateUserCommand).Assembly;

        // Act
        var result = services.AddNexus(assembly);

        // Assert
        result.ShouldBeSameAs(services);
    }

    [Fact]
    public void AddNexus_WithMultipleAssemblies_ShouldReturnServiceCollectionForChaining()
    {
        // Arrange
        var services = new ServiceCollection();
        var assembly1 = typeof(CreateUserCommand).Assembly;
        var assembly2 = typeof(DependencyInjectionTests).Assembly;

        // Act
        var result = services.AddNexus(assembly1, assembly2);

        // Assert
        result.ShouldBeSameAs(services);
    }
} 