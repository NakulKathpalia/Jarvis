namespace Jarvis.Core.Agents.Coding.Runnable;

/// <summary>
/// Represents a runnable task request.
/// </summary>
public sealed class RunnableRequest
{
    /// <summary>
    /// Gets or sets the repository path.
    /// </summary>
    public string RepositoryPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the prompt.
    /// </summary>
    public string Prompt { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether repository writes are allowed.
    /// </summary>
    public bool ApplyToRepository { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether explicit approval was provided.
    /// </summary>
    public bool ExplicitApproval { get; set; }
}
