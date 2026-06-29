namespace Jarvis.Core.Agents.Coding.Models;

/// <summary>
/// Represents indexed repository configuration information.
/// </summary>
public sealed class ConfigurationIndex
{
    /// <summary>
    /// Gets or sets indexed configurations.
    /// </summary>
    public List<RepositoryConfiguration> Configurations { get; set; } = [];
}
