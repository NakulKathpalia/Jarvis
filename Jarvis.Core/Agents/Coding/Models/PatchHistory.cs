namespace Jarvis.Core.Agents.Coding.Models;

/// <summary>
/// Represents rollback information for a patch execution.
/// </summary>
public sealed class PatchHistory
{
    /// <summary>
    /// Gets or sets the history identifier.
    /// </summary>
    public string HistoryId { get; set; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// Gets or sets file snapshots captured before patch execution.
    /// </summary>
    public Dictionary<string, string?> OriginalFileContents { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets or sets moved file pairs from destination to original source.
    /// </summary>
    public Dictionary<string, string> MoveRollbackMap { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets or sets applied operation identifiers.
    /// </summary>
    public List<string> AppliedOperationIds { get; set; } = [];
}
