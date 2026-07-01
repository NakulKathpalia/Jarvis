namespace Jarvis.Core.Agents.Coding.Experience;

/// <summary>
/// Captures learned project coding conventions.
/// </summary>
public sealed class ProjectCodingProfile
{
    /// <summary>
    /// Gets or sets the repository.
    /// </summary>
    public string Repository { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether file-scoped namespaces are used.
    /// </summary>
    public bool UsesFileScopedNamespaces { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether async is commonly used.
    /// </summary>
    public bool UsesAsync { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether dependency injection is used.
    /// </summary>
    public bool UsesDependencyInjection { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether minimal APIs are used.
    /// </summary>
    public bool UsesMinimalApis { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether nullable reference types are used.
    /// </summary>
    public bool UsesNullableReferenceTypes { get; set; }

    /// <summary>
    /// Gets preferred style notes.
    /// </summary>
    public List<string> PreferredStyle { get; } = [];

    /// <summary>
    /// Renders profile text suitable for AI context.
    /// </summary>
    public string ToContextText()
    {
        return $"Project profile: file-scoped namespaces={UsesFileScopedNamespaces}, async={UsesAsync}, " +
            $"DI={UsesDependencyInjection}, minimal APIs={UsesMinimalApis}, nullable={UsesNullableReferenceTypes}. " +
            $"Style: {string.Join("; ", PreferredStyle)}";
    }
}
