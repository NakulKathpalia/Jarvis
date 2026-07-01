namespace Jarvis.Core.Agents.Coding.FileGeneration;

using Jarvis.Core.Agents.Coding.Runnable;

/// <summary>
/// Describes a project generation template.
/// </summary>
public sealed class ProjectTemplate
{
    /// <summary>
    /// Gets or sets the task type.
    /// </summary>
    public RunnableTaskType TaskType { get; set; }

    /// <summary>
    /// Gets or sets the display name.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;
}
