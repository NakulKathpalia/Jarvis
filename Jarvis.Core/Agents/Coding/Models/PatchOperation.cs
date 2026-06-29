namespace Jarvis.Core.Agents.Coding.Models;

/// <summary>
/// Represents one deterministic patch operation.
/// </summary>
public sealed class PatchOperation
{
    /// <summary>
    /// Gets or sets the operation identifier.
    /// </summary>
    public string OperationId { get; set; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// Gets or sets the operation type.
    /// </summary>
    public PatchOperationType Type { get; set; }

    /// <summary>
    /// Gets or sets the target file path.
    /// </summary>
    public string TargetPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the destination path for move or extract operations.
    /// </summary>
    public string DestinationPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the one-based start line.
    /// </summary>
    public int StartLine { get; set; }

    /// <summary>
    /// Gets or sets the one-based end line.
    /// </summary>
    public int EndLine { get; set; }

    /// <summary>
    /// Gets or sets text to insert or write.
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets text to search for.
    /// </summary>
    public string SearchText { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets replacement text.
    /// </summary>
    public string ReplaceText { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the symbol name to rename.
    /// </summary>
    public string SymbolName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the new symbol name.
    /// </summary>
    public string NewName { get; set; } = string.Empty;
}
