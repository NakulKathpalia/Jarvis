namespace Jarvis.Core.Agents.Coding.Runnable;

/// <summary>
/// Defines runnable task types.
/// </summary>
public enum RunnableTaskType
{
    /// <summary>
    /// The request is not a runnable task.
    /// </summary>
    None,

    /// <summary>
    /// A static HTML, CSS, and JavaScript page.
    /// </summary>
    StaticHtml,

    /// <summary>
    /// A React UI application.
    /// </summary>
    React
}
