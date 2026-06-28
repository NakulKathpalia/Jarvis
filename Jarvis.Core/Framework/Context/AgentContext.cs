namespace Jarvis.Core.Framework.Context;

using Jarvis.Core.Framework.Models;

/// <summary>
/// Represents the context provided to an executing agent.
/// </summary>
public sealed class AgentContext
{
    /// <summary>
    /// Gets or sets the selected agent name.
    /// </summary>
    public string AgentName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the shared execution context.
    /// </summary>
    public ExecutionContext ExecutionContext { get; set; } = new();
}
