namespace Jarvis.Core.Agents.Coding.Services;

/// <summary>
/// Provides shared repository path filtering rules.
/// </summary>
public static class RepositoryPathRules
{
    private static readonly HashSet<string> IgnoredFolders = new(StringComparer.OrdinalIgnoreCase)
    {
        ".git",
        "bin",
        "obj",
        "node_modules"
    };

    /// <summary>
    /// Determines whether a directory should be ignored during scans.
    /// </summary>
    /// <param name="directoryPath">The directory path.</param>
    /// <returns><c>true</c> when the directory should be skipped.</returns>
    public static bool ShouldIgnoreDirectory(string directoryPath)
    {
        return IgnoredFolders.Contains(System.IO.Path.GetFileName(directoryPath));
    }

    /// <summary>
    /// Gets a repository-relative path.
    /// </summary>
    /// <param name="rootPath">The repository root path.</param>
    /// <param name="path">The absolute path.</param>
    /// <returns>The repository-relative path.</returns>
    public static string GetRelativePath(string rootPath, string path)
    {
        return System.IO.Path.GetRelativePath(rootPath, path).Replace('\\', '/');
    }
}
