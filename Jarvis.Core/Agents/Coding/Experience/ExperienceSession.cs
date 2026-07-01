namespace Jarvis.Core.Agents.Coding.Experience;

using Jarvis.Core.Agents.Coding.Models;
using Jarvis.Core.Agents.Coding.Review.Quality;

/// <summary>
/// Captures one coding session for engineering experience.
/// </summary>
public sealed class ExperienceSession
{
    /// <summary>
    /// Gets or sets the session identifier.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// Gets or sets the user request.
    /// </summary>
    public string UserRequest { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the repository path or name.
    /// </summary>
    public string Repository { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the model name.
    /// </summary>
    public string Model { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the provider name.
    /// </summary>
    public string Provider { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the original prompt.
    /// </summary>
    public string Prompt { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the compressed prompt.
    /// </summary>
    public string CompressedPrompt { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets context text.
    /// </summary>
    public string Context { get; set; } = string.Empty;

    /// <summary>
    /// Gets selected files.
    /// </summary>
    public List<string> SelectedFiles { get; } = [];

    /// <summary>
    /// Gets selected symbols.
    /// </summary>
    public List<string> SelectedSymbols { get; } = [];

    /// <summary>
    /// Gets coding plan lines.
    /// </summary>
    public List<string> CodingPlan { get; } = [];

    /// <summary>
    /// Gets or sets patch preview text.
    /// </summary>
    public string PatchPreview { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets applied patch text.
    /// </summary>
    public string AppliedPatch { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets build result.
    /// </summary>
    public BuildResult? BuildResult { get; set; }

    /// <summary>
    /// Gets or sets review result.
    /// </summary>
    public CodeReviewResult? ReviewResult { get; set; }

    /// <summary>
    /// Gets or sets session duration.
    /// </summary>
    public TimeSpan Duration { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the session succeeded.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets failure reason.
    /// </summary>
    public string FailureReason { get; set; } = string.Empty;

    /// <summary>
    /// Gets warnings.
    /// </summary>
    public List<string> Warnings { get; } = [];

    /// <summary>
    /// Gets or sets a value indicating whether the user approved changes.
    /// </summary>
    public bool UserApproval { get; set; }

    /// <summary>
    /// Gets or sets when the session occurred.
    /// </summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
}
