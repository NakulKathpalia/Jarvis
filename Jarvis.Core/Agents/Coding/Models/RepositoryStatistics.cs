namespace Jarvis.Core.Agents.Coding.Models;

/// <summary>
/// Represents factual repository statistics.
/// </summary>
public sealed class RepositoryStatistics
{
    /// <summary>
    /// Gets or sets the project count.
    /// </summary>
    public int ProjectCount { get; set; }

    /// <summary>
    /// Gets or sets the folder count.
    /// </summary>
    public int FolderCount { get; set; }

    /// <summary>
    /// Gets or sets the file count.
    /// </summary>
    public int FileCount { get; set; }

    /// <summary>
    /// Gets or sets the large file count.
    /// </summary>
    public int LargeFileCount { get; set; }

    /// <summary>
    /// Gets or sets the average files per project.
    /// </summary>
    public double AverageProjectSize { get; set; }
}
