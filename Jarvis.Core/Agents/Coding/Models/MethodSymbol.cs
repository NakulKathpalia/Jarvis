namespace Jarvis.Core.Agents.Coding.Models;

/// <summary>
/// Represents a method symbol.
/// </summary>
public sealed class MethodSymbol : CodeSymbol
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MethodSymbol"/> class.
    /// </summary>
    public MethodSymbol()
    {
        Kind = "Method";
    }
}
