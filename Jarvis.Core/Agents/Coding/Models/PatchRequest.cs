namespace Jarvis.Core.Agents.Coding.Models;

/// <summary>
/// Represents a deterministic patch request.
/// </summary>
public sealed class PatchRequest
{
    /// <summary>
    /// Gets or sets the request identifier.
    /// </summary>
    public string RequestId { get; set; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// Gets or sets a value indicating whether execution should avoid writes.
    /// </summary>
    public bool DryRun { get; set; }

    /// <summary>
    /// Gets or sets the patch operations.
    /// </summary>
    public List<PatchOperation> Operations { get; set; } = [];
}
