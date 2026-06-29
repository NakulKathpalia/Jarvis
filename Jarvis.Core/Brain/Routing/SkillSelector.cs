namespace Jarvis.Core.Brain.Routing;

using Jarvis.Core.Brain.Interfaces;
using Jarvis.Core.Brain.Models;
using Jarvis.Core.Framework.Skills;

/// <summary>
/// Selects a skill from supplied candidates.
/// </summary>
public sealed class SkillSelector : ISkillSelector
{
    /// <inheritdoc />
    public RoutingDecision SelectSkill(RoutingDecision decision, IReadOnlyCollection<ISkill> skills)
    {
        ArgumentNullException.ThrowIfNull(decision);

        var skill = skills.FirstOrDefault();
        decision.SkillName = skill?.Name ?? string.Empty;
        return decision;
    }
}
