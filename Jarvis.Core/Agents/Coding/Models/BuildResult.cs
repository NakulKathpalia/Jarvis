namespace Jarvis.Core.Agents.Coding.Models;

/// <summary>
/// Represents the result of a build run.
/// </summary>
public sealed class BuildResult
{
    /// <summary>
    /// Gets or sets a value indicating whether the build succeeded.
    /// </summary>
    public bool Succeeded { get; set; }

    /// <summary>
    /// Gets or sets the build configuration used.
    /// </summary>
    public BuildConfiguration Configuration { get; set; } = new();

    /// <summary>
    /// Gets or sets compiler errors.
    /// </summary>
    public List<CompilerError> Errors { get; set; } = [];

    /// <summary>
    /// Gets or sets compiler warnings.
    /// </summary>
    public List<CompilerWarning> Warnings { get; set; } = [];

    /// <summary>
    /// Gets or sets build statistics.
    /// </summary>
    public BuildStatistics Statistics { get; set; } = new();

    /// <summary>
    /// Gets or sets raw build output.
    /// </summary>
    public string Output { get; set; } = string.Empty;
}
