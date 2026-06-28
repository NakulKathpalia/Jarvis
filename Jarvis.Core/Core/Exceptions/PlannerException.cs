namespace Jarvis.Core.Core.Exceptions;

/// <summary>
/// Represents a task planning failure.
/// </summary>
public sealed class PlannerException : FrameworkException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PlannerException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    public PlannerException(string message)
        : base(message)
    {
    }
}
