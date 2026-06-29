namespace Jarvis.Core.Agents.Coding.Autonomous;

using Jarvis.Core.Agents.Coding.Models;

/// <summary>
/// Represents an autonomous coding request.
/// </summary>
public sealed class AutonomousCodingRequest
{
    /// <summary>
    /// Gets or sets the repository path.
    /// </summary>
    public string RepositoryPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user request.
    /// </summary>
    public string UserRequest { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether patch application is explicitly approved.
    /// </summary>
    public bool ExplicitApproval { get; set; }

    /// <summary>
    /// Gets or sets execution mode.
    /// </summary>
    public CodingExecutionMode Mode { get; set; } = CodingExecutionMode.PreviewOnly;

    /// <summary>
    /// Gets or sets maximum repair iterations.
    /// </summary>
    public int MaxIterations { get; set; } = 3;

    /// <summary>
    /// Gets or sets an optional model name.
    /// </summary>
    public string ModelName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets an approved deterministic patch request.
    /// </summary>
    public PatchRequest? ApprovedPatchRequest { get; set; }
}
