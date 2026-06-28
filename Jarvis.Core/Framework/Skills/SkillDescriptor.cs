namespace Jarvis.Core.Framework.Skills;

/// <summary>
/// Describes metadata for a Jarvis skill.
/// </summary>
public sealed class SkillDescriptor
{
    /// <summary>
    /// Gets or sets the unique skill name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display name shown to humans.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the skill description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the skill version.
    /// </summary>
    public string Version { get; set; } = "1.0.0";
}
