namespace Jarvis.Core.Agents.Coding.Models;

/// <summary>
/// Represents factual coding context selected for a request.
/// </summary>
public sealed class ContextPackage
{
    /// <summary>
    /// Gets or sets the original context request.
    /// </summary>
    public ContextRequest Request { get; set; } = new();

    /// <summary>
    /// Gets or sets relevant projects.
    /// </summary>
    public List<RelevantProject> RelevantProjects { get; set; } = [];

    /// <summary>
    /// Gets or sets relevant files.
    /// </summary>
    public List<RelevantFile> RelevantFiles { get; set; } = [];

    /// <summary>
    /// Gets or sets relevant symbols.
    /// </summary>
    public List<RelevantSymbol> RelevantSymbols { get; set; } = [];

    /// <summary>
    /// Gets or sets related interfaces.
    /// </summary>
    public List<RelevantSymbol> RelatedInterfaces { get; set; } = [];

    /// <summary>
    /// Gets or sets related namespaces.
    /// </summary>
    public List<string> RelatedNamespaces { get; set; } = [];

    /// <summary>
    /// Gets or sets related dependencies.
    /// </summary>
    public List<string> RelatedDependencies { get; set; } = [];

    /// <summary>
    /// Gets or sets context statistics.
    /// </summary>
    public ContextStatistics Statistics { get; set; } = new();
}
