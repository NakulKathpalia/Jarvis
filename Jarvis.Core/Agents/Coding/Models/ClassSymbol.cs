namespace Jarvis.Core.Agents.Coding.Models;

/// <summary>
/// Represents a class symbol.
/// </summary>
public sealed class ClassSymbol : CodeSymbol
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ClassSymbol"/> class.
    /// </summary>
    public ClassSymbol()
    {
        Kind = "Class";
    }
}
