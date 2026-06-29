namespace Jarvis.Core.Agents.Coding.Services;

using Jarvis.Core.Agents.Coding.Models;

/// <summary>
/// Scans repository folders recursively.
/// </summary>
public sealed class FolderScanner
{
    /// <summary>
    /// Scans folders under a repository root.
    /// </summary>
    /// <param name="rootPath">The repository root path.</param>
    /// <returns>The discovered folders.</returns>
    public IReadOnlyList<RepositoryFolder> Scan(string rootPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rootPath);

        var folders = new List<RepositoryFolder>();
        foreach (var directory in EnumerateDirectories(rootPath))
        {
            folders.Add(new RepositoryFolder
            {
                Path = directory,
                RelativePath = RepositoryPathRules.GetRelativePath(rootPath, directory),
                Name = Path.GetFileName(directory)
            });
        }

        return folders;
    }

    private static IEnumerable<string> EnumerateDirectories(string rootPath)
    {
        foreach (var directory in Directory.EnumerateDirectories(rootPath))
        {
            if (RepositoryPathRules.ShouldIgnoreDirectory(directory))
            {
                continue;
            }

            yield return directory;
            foreach (var child in EnumerateDirectories(directory))
            {
                yield return child;
            }
        }
    }
}
