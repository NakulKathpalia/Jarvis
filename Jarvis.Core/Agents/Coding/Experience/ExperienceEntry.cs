namespace Jarvis.Core.Agents.Coding.Experience;

/// <summary>
/// Represents one learned coding experience.
/// </summary>
public sealed class ExperienceEntry
{
    /// <summary>
    /// Gets or sets the entry identifier.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// Gets or sets the entry category.
    /// </summary>
    public ExperienceCategory Category { get; set; } = ExperienceCategory.General;

    /// <summary>
    /// Gets or sets the entry text.
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the related file path.
    /// </summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the related symbol name.
    /// </summary>
    public string SymbolName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets when the entry was created.
    /// </summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
}
