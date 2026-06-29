namespace Jarvis.Core.Agents.Voice.Models;

/// <summary>
/// Represents a generic voice command.
/// </summary>
public sealed class VoiceCommand
{
    /// <summary>
    /// Gets or sets the command name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the command input.
    /// </summary>
    public string Input { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether confirmation is required.
    /// </summary>
    public bool RequiresConfirmation { get; set; }
}
