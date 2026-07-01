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
    /// A static HTML project.
    /// </summary>
    Html,

    /// <summary>
    /// A CSS-focused project.
    /// </summary>
    Css,

    /// <summary>
    /// A JavaScript project.
    /// </summary>
    JavaScript,

    /// <summary>
    /// A static HTML, CSS, and JavaScript page.
    /// </summary>
    StaticHtml,

    /// <summary>
    /// A React UI application.
    /// </summary>
    React,

    /// <summary>
    /// A Vue UI application.
    /// </summary>
    Vue,

    /// <summary>
    /// An Angular UI application.
    /// </summary>
    Angular,

    /// <summary>
    /// A console application.
    /// </summary>
    Console,

    /// <summary>
    /// An API project.
    /// </summary>
    Api,

    /// <summary>
    /// A library project.
    /// </summary>
    Library
}
