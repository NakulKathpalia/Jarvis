namespace Jarvis.Core.Agents.Coding.FileGeneration;

using Jarvis.Core.Agents.Coding.Runnable;

/// <summary>
/// Represents a generated project.
/// </summary>
public sealed class GeneratedProject
{
    /// <summary>
    /// Gets or sets the project type.
    /// </summary>
    public RunnableTaskType ProjectType { get; set; }

    /// <summary>
    /// Gets generated files.
    /// </summary>
    public List<GeneratedFile> Files { get; } = [];
}
