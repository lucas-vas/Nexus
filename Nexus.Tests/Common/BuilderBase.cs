namespace Nexus.Tests.Common;

/// <summary>
/// Base class for all builders in the test project.
/// </summary>
/// <typeparam name="T">The type of object to build.</typeparam>
public abstract class BuilderBase<T>
{
    /// <summary>
    /// Builds the object of type T.
    /// </summary>
    /// <returns>The built object.</returns>
    public abstract T Build();
} 