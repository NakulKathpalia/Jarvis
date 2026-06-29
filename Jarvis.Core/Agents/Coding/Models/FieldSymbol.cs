namespace Jarvis.Core.Agents.Coding.Models;

/// <summary>
/// Represents a field symbol.
/// </summary>
public sealed class FieldSymbol : CodeSymbol
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FieldSymbol"/> class.
    /// </summary>
    public FieldSymbol()
    {
        Kind = "Field";
    }
}
