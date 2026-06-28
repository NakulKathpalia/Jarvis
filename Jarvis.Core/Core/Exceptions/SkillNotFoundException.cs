namespace Jarvis.Core.Core.Exceptions;

/// <summary>
/// Represents a failure to resolve a requested skill.
/// </summary>
public sealed class SkillNotFoundException : FrameworkException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SkillNotFoundException"/> class.
    /// </summary>
    /// <param name="skillName">The unresolved skill name.</param>
    public SkillNotFoundException(string skillName)
        : base($"Skill was not found: {skillName}")
    {
        SkillName = skillName;
    }

    /// <summary>
    /// Gets the unresolved skill name.
    /// </summary>
    public string SkillName { get; }
}
