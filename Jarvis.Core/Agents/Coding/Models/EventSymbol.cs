namespace Jarvis.Core.Agents.Coding.Models;

/// <summary>
/// Represents an event symbol.
/// </summary>
public sealed class EventSymbol : CodeSymbol
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EventSymbol"/> class.
    /// </summary>
    public EventSymbol()
    {
        Kind = "Event";
    }
}
