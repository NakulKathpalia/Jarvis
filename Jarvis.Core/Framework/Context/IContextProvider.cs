namespace Jarvis.Core.Framework.Context;

using Jarvis.Core.Framework.Models;

/// <summary>
/// Defines a provider that creates execution context for task requests.
/// </summary>
public interface IContextProvider
{
    /// <summary>
    /// Creates a new execution context for the supplied request.
    /// </summary>
    /// <param name="request">The request being executed.</param>
    /// <returns>A new execution context.</returns>
    ExecutionContext CreateContext(TaskRequest request);
}
