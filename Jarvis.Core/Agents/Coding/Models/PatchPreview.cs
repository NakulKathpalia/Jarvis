namespace Jarvis.Core.Agents.Coding.Models;

/// <summary>
/// Represents a human-readable patch preview.
/// </summary>
public sealed class PatchPreview
{
    /// <summary>
    /// Gets or sets preview lines.
    /// </summary>
    public List<string> Lines { get; set; } = [];

    /// <summary>
    /// Gets the preview text.
    /// </summary>
    public string Text => string.Join(Environment.NewLine, Lines);
}
