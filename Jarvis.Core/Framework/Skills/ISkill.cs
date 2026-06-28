namespace Jarvis.Core.Framework.Skills;

using Jarvis.Core.Framework.Context;
using Jarvis.Core.Framework.Models;
using Jarvis.Core.Framework.Routing;

/// <summary>
/// Defines a reusable skill that an agent can invoke.
/// </summary>
public interface ISkill
{
    /// <summary>
    /// Gets the unique skill name.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Executes the skill with access to the tool executor.
    /// </summary>
    /// <param name="context">The active agent context.</param>
    /// <param name="toolExecutor">The tool executor available to the skill.</param>
    /// <param name="cancellationToken">A token that cancels execution.</param>
    /// <returns>The result of the tool-backed skill execution.</returns>
    Task<ToolResult> ExecuteAsync(
        AgentContext context,
        IToolExecutor toolExecutor,
        CancellationToken cancellationToken = default);
}
