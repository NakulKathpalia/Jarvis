namespace Jarvis.Core.Agents.Coding.Assistant;

using Jarvis.Core.Agents.Coding.Models;

/// <summary>
/// Represents a local coding assistant request.
/// </summary>
public sealed class CodingAssistantRequest
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
    /// Gets or sets the assistant mode.
    /// </summary>
    public CodingAssistantMode Mode { get; set; } = CodingAssistantMode.SuggestPatch;

    /// <summary>
    /// Gets or sets an optional model name.
    /// </summary>
    public string ModelName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets an optional build result for build fix mode.
    /// </summary>
    public BuildResult? BuildResult { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether apply has explicit approval.
    /// </summary>
    public bool ExplicitApproval { get; set; }

    /// <summary>
    /// Gets or sets a deterministic approved patch request.
    /// </summary>
    public PatchRequest? ApprovedPatchRequest { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether runnable output may be written to the repository.
    /// </summary>
    public bool ApplyToRepository { get; set; }
}
