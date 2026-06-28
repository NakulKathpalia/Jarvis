namespace Jarvis.Core.Framework.Skills;

/// <summary>
/// Describes metadata for a Jarvis tool.
/// </summary>
public sealed class ToolDescriptor
{
    /// <summary>
    /// Gets or sets the unique tool name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display name shown to humans.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the tool description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the tool version.
    /// </summary>
    public string Version { get; set; } = "1.0.0";
}
