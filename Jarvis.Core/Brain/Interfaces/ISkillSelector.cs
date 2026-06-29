namespace Jarvis.Core.Brain.Interfaces;

using Jarvis.Core.Brain.Models;
using Jarvis.Core.Framework.Skills;

/// <summary>
/// Defines skill selection for a routing decision.
/// </summary>
public interface ISkillSelector
{
    /// <summary>
    /// Selects a compatible skill from supplied candidates.
    /// </summary>
    /// <param name="decision">The current routing decision.</param>
    /// <param name="skills">The candidate skills.</param>
    /// <returns>The updated routing decision.</returns>
    RoutingDecision SelectSkill(RoutingDecision decision, IReadOnlyCollection<ISkill> skills);
}
