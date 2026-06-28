namespace Jarvis.Core.Framework.Context;

using Jarvis.Core.Framework.Models;

/// <summary>
/// Creates framework execution context for incoming requests.
/// </summary>
public sealed class ContextManager : IContextProvider
{
    /// <inheritdoc />
    public ExecutionContext CreateContext(TaskRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        return new ExecutionContext
        {
            Request = request
        };
    }
}
