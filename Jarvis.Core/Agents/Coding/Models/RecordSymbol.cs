namespace Jarvis.Core.Agents.Coding.Models;

/// <summary>
/// Represents a record symbol.
/// </summary>
public sealed class RecordSymbol : CodeSymbol
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RecordSymbol"/> class.
    /// </summary>
    public RecordSymbol()
    {
        Kind = "Record";
    }
}
