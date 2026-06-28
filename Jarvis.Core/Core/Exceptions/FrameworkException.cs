namespace Jarvis.Core.Core.Exceptions;

/// <summary>
/// Represents a base exception for Jarvis framework runtime failures.
/// </summary>
public class FrameworkException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FrameworkException"/> class.
    /// </summary>
    public FrameworkException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FrameworkException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    public FrameworkException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FrameworkException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="innerException">The inner exception.</param>
    public FrameworkException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
