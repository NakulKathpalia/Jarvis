namespace Jarvis.Core.Agents.Coding.Models;

/// <summary>
/// Represents supported build tool configuration.
/// </summary>
public sealed class BuildConfiguration
{
    /// <summary>
    /// Gets or sets the build tool name.
    /// </summary>
    public string Tool { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the executable command.
    /// </summary>
    public string Command { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets command arguments.
    /// </summary>
    public string Arguments { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the working directory.
    /// </summary>
    public string WorkingDirectory { get; set; } = string.Empty;
}
