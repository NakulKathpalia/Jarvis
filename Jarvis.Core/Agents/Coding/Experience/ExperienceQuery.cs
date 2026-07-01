namespace Jarvis.Core.Agents.Coding.Experience;

/// <summary>
/// Represents an experience lookup query.
/// </summary>
public sealed class ExperienceQuery
{
    /// <summary>
    /// Gets or sets the user request.
    /// </summary>
    public string UserRequest { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the repository.
    /// </summary>
    public string Repository { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets maximum result count.
    /// </summary>
    public int MaxResults { get; set; } = 5;
}
