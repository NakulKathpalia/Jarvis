namespace Jarvis.Core.Agents.Coding.Models;

/// <summary>
/// Represents indexed repository project information.
/// </summary>
public sealed class ProjectIndex
{
    /// <summary>
    /// Gets or sets indexed projects.
    /// </summary>
    public List<RepositoryProject> Projects { get; set; } = [];
}
