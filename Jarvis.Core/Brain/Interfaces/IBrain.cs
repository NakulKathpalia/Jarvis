namespace Jarvis.Core.Brain.Interfaces;

using Jarvis.Core.Brain.Models;
using Jarvis.Core.Framework.Registry;
using Jarvis.Core.Framework.Skills;
using Jarvis.Core.Shared.Interfaces;

/// <summary>
/// Defines the non-executing Brain decision engine.
/// </summary>
public interface IBrain
{
    /// <summary>
    /// Creates an execution plan without executing it.
    /// </summary>
    /// <param name="input">The user input.</param>
    /// <param name="registry">The agent registry.</param>
    /// <param name="skills">The candidate skills.</param>
    /// <param name="tools">The candidate tools.</param>
    /// <returns>The execution plan.</returns>
    ExecutionPlan Plan(
        string input,
        IAgentRegistry registry,
        IReadOnlyCollection<ISkill>? skills = null,
        IReadOnlyCollection<ITool>? tools = null);
}
