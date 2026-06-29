namespace Jarvis.Core.Agents.Coding.Models;

/// <summary>
/// Represents supported deterministic patch operation types.
/// </summary>
public enum PatchOperationType
{
    /// <summary>
    /// Inserts text into a file.
    /// </summary>
    Insert,

    /// <summary>
    /// Replaces text or a line range.
    /// </summary>
    Replace,

    /// <summary>
    /// Deletes text, a line range, or a file.
    /// </summary>
    Delete,

    /// <summary>
    /// Renames a symbol by exact token match.
    /// </summary>
    Rename,

    /// <summary>
    /// Moves a file.
    /// </summary>
    Move,

    /// <summary>
    /// Extracts a line range into another file.
    /// </summary>
    Extract,

    /// <summary>
    /// Creates a file.
    /// </summary>
    CreateFile,

    /// <summary>
    /// Deletes a file.
    /// </summary>
    DeleteFile,

    /// <summary>
    /// Adds or updates a using/import statement.
    /// </summary>
    UpdateUsing,

    /// <summary>
    /// Updates a namespace declaration.
    /// </summary>
    UpdateNamespace
}
