namespace Jarvis.Core.Agents.Coding.Services;

using Jarvis.Core.Agents.Coding.Models;

/// <summary>
/// Scans repository files recursively.
/// </summary>
public sealed class FileScanner
{
    private readonly LanguageDetector languageDetector;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileScanner"/> class.
    /// </summary>
    /// <param name="languageDetector">The language detector.</param>
    public FileScanner(LanguageDetector? languageDetector = null)
    {
        this.languageDetector = languageDetector ?? new LanguageDetector();
    }

    /// <summary>
    /// Scans files under a repository root.
    /// </summary>
    /// <param name="rootPath">The repository root path.</param>
    /// <returns>The discovered files.</returns>
    public IReadOnlyList<RepositoryFile> Scan(string rootPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rootPath);

        var files = new List<RepositoryFile>();
        foreach (var file in EnumerateFiles(rootPath))
        {
            var info = new FileInfo(file);
            files.Add(new RepositoryFile
            {
                Path = file,
                RelativePath = RepositoryPathRules.GetRelativePath(rootPath, file),
                Name = info.Name,
                Extension = info.Extension.ToLowerInvariant(),
                Language = languageDetector.Detect(file),
                SizeBytes = info.Length
            });
        }

        return files;
    }

    private static IEnumerable<string> EnumerateFiles(string rootPath)
    {
        foreach (var file in Directory.EnumerateFiles(rootPath))
        {
            yield return file;
        }

        foreach (var directory in Directory.EnumerateDirectories(rootPath))
        {
            if (RepositoryPathRules.ShouldIgnoreDirectory(directory))
            {
                continue;
            }

            foreach (var file in EnumerateFiles(directory))
            {
                yield return file;
            }
        }
    }
}
