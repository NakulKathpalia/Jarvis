using System.Diagnostics;

namespace Jarvis.Services;

public sealed class FileIndexService
{
    private static readonly HashSet<string> ExcludedDirectoryNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "bin",
        "obj",
        "node_modules",
        ".git",
        ".next",
        "models",
        "tools"
    };

    private static readonly HashSet<string> TextExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".txt",
        ".md",
        ".json",
        ".jsonc",
        ".jsonl",
        ".xml",
        ".yaml",
        ".yml",
        ".csv",
        ".ts",
        ".tsx",
        ".js",
        ".jsx",
        ".mjs",
        ".cjs",
        ".cs",
        ".csproj",
        ".sln",
        ".props",
        ".targets",
        ".html",
        ".htm",
        ".css",
        ".scss",
        ".less",
        ".razor",
        ".cshtml",
        ".config",
        ".ini",
        ".log",
        ".py",
        ".java",
        ".cpp",
        ".c",
        ".h",
        ".hpp",
        ".sql",
        ".env"
    };

    private readonly SettingsService _settingsService;
    private readonly List<string> _indexedFiles = [];

    public FileIndexService(SettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    public IReadOnlyCollection<string> IndexedFiles => _indexedFiles;

    public Task<int> RebuildAsync(CancellationToken cancellationToken = default)
    {
        var root = GetRootPath();
        if (!Directory.Exists(root))
        {
            _indexedFiles.Clear();
            return Task.FromResult(0);
        }

        _indexedFiles.Clear();
        foreach (var file in EnumerateFiles(root, cancellationToken))
        {
            _indexedFiles.Add(file);
        }

        return Task.FromResult(_indexedFiles.Count);
    }

    public IEnumerable<string> Search(string query)
    {
        var normalizedQuery = query.Trim();
        if (string.IsNullOrWhiteSpace(normalizedQuery))
        {
            return Enumerable.Empty<string>();
        }

        return _indexedFiles
            .Where(file => Path.GetFileName(file).Contains(normalizedQuery, StringComparison.OrdinalIgnoreCase)
                || file.Contains(normalizedQuery, StringComparison.OrdinalIgnoreCase))
            .Take(20);
    }

    public IReadOnlyList<FileSearchResult> SearchDetailed(
        string query,
        int maxResults = 25,
        long maxFileSizeBytes = 1024 * 1024)
    {
        var normalizedQuery = query.Trim();
        if (string.IsNullOrWhiteSpace(normalizedQuery))
        {
            return [];
        }

        var results = new Dictionary<string, FileSearchResult>(StringComparer.OrdinalIgnoreCase);

        foreach (var file in _indexedFiles)
        {
            if (results.Count >= maxResults)
            {
                break;
            }

            var fileInfo = new FileInfo(file);
            var relativePath = GetRelativePath(file);
            var pathMatch = fileInfo.Name.Contains(normalizedQuery, StringComparison.OrdinalIgnoreCase)
                || relativePath.Contains(normalizedQuery, StringComparison.OrdinalIgnoreCase);

            if (pathMatch)
            {
                results[file] = new FileSearchResult(
                    file,
                    fileInfo.Name,
                    relativePath,
                    "path",
                    null,
                    fileInfo.Exists ? fileInfo.Length : 0,
                    fileInfo.Exists ? fileInfo.LastWriteTimeUtc : DateTime.MinValue);
            }

            if (results.Count >= maxResults)
            {
                break;
            }

            if (!CanSearchContent(fileInfo, maxFileSizeBytes))
            {
                continue;
            }

            var contentMatch = TryReadContentMatch(file, normalizedQuery, out var snippet, out var lineNumber);
            if (!contentMatch)
            {
                continue;
            }

            results[file] = new FileSearchResult(
                file,
                fileInfo.Name,
                relativePath,
                pathMatch
                    ? $"path + content line {lineNumber.GetValueOrDefault()}"
                    : lineNumber.HasValue
                        ? $"content line {lineNumber.Value}"
                        : "content",
                snippet,
                fileInfo.Exists ? fileInfo.Length : 0,
                fileInfo.Exists ? fileInfo.LastWriteTimeUtc : DateTime.MinValue);
        }

        return results.Values
            .OrderByDescending(result => result.MatchType.StartsWith("path", StringComparison.OrdinalIgnoreCase))
            .ThenByDescending(result => result.LastWriteTimeUtc)
            .Take(maxResults)
            .ToList();
    }

    public bool OpenFile(string path)
    {
        var fullPath = ResolveAllowedPath(path, mustBeDirectory: false);
        if (fullPath is null || !File.Exists(fullPath))
        {
            return false;
        }

        return StartProcess(fullPath);
    }

    public bool OpenContainingFolder(string path)
    {
        var fullPath = ResolveAllowedPath(path, mustBeDirectory: false);
        if (fullPath is null || !File.Exists(fullPath))
        {
            return false;
        }

        var directory = Path.GetDirectoryName(fullPath);
        if (string.IsNullOrWhiteSpace(directory) || !Directory.Exists(directory))
        {
            return false;
        }

        if (OperatingSystem.IsWindows())
        {
            return StartProcess("explorer.exe", $"/select,\"{fullPath}\"");
        }

        return StartProcess(directory);
    }

    private IEnumerable<string> EnumerateFiles(string root, CancellationToken cancellationToken)
    {
        var stack = new Stack<string>();
        stack.Push(root);

        while (stack.Count > 0)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                yield break;
            }

            var currentDirectory = stack.Pop();
            IEnumerable<string> files;
            IEnumerable<string> subDirectories;

            try
            {
                files = Directory.EnumerateFiles(currentDirectory, "*", SearchOption.TopDirectoryOnly);
                subDirectories = Directory.EnumerateDirectories(currentDirectory, "*", SearchOption.TopDirectoryOnly);
            }
            catch
            {
                continue;
            }

            foreach (var file in files)
            {
                if (ShouldSkipPath(file))
                {
                    continue;
                }

                yield return file;
            }

            foreach (var subDirectory in subDirectories)
            {
                if (ShouldSkipDirectory(subDirectory))
                {
                    continue;
                }

                stack.Push(subDirectory);
            }
        }
    }

    private bool ShouldSkipDirectory(string path)
    {
        return PathSegments(path).Any(segment => ExcludedDirectoryNames.Contains(segment));
    }

    private bool ShouldSkipPath(string path)
    {
        return PathSegments(path).Any(segment => ExcludedDirectoryNames.Contains(segment));
    }

    private bool CanSearchContent(FileInfo fileInfo, long maxFileSizeBytes)
    {
        if (!fileInfo.Exists || fileInfo.Length <= 0 || fileInfo.Length > maxFileSizeBytes)
        {
            return false;
        }

        var extension = fileInfo.Extension;
        if (string.IsNullOrWhiteSpace(extension))
        {
            return false;
        }

        return TextExtensions.Contains(extension);
    }

    private static bool TryReadContentMatch(string filePath, string query, out string? snippet, out int? lineNumber)
    {
        snippet = null;
        lineNumber = null;

        try
        {
            var lines = File.ReadLines(filePath);
            var currentLine = 0;
            foreach (var line in lines)
            {
                currentLine++;
                if (!line.Contains(query, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                lineNumber = currentLine;
                snippet = BuildSnippet(line, query);
                return true;
            }
        }
        catch
        {
            return false;
        }

        return false;
    }

    private static string BuildSnippet(string line, string query)
    {
        var text = line.Trim();
        if (text.Length <= 180)
        {
            return text;
        }

        var index = text.IndexOf(query, StringComparison.OrdinalIgnoreCase);
        if (index < 0)
        {
            return text[..180] + "...";
        }

        var start = Math.Max(0, index - 60);
        var length = Math.Min(180, text.Length - start);
        var snippet = text.Substring(start, length);
        if (start > 0)
        {
            snippet = "..." + snippet;
        }

        if (start + length < text.Length)
        {
            snippet += "...";
        }

        return snippet;
    }

    private string GetRelativePath(string path)
    {
        var root = GetRootPath();
        try
        {
            return Path.GetRelativePath(root, path);
        }
        catch
        {
            return path;
        }
    }

    private string GetRootPath()
    {
        var configuredRoot = _settingsService.Current.FileIndexRoot;
        if (string.IsNullOrWhiteSpace(configuredRoot))
        {
            return AppContext.BaseDirectory;
        }

        return Path.GetFullPath(Path.IsPathRooted(configuredRoot)
            ? configuredRoot
            : Path.Combine(AppContext.BaseDirectory, configuredRoot));
    }

    private string? ResolveAllowedPath(string path, bool mustBeDirectory)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        var fullPath = Path.GetFullPath(path);
        var root = GetRootPath();
        if (!IsUnderRoot(fullPath, root))
        {
            return null;
        }

        if (mustBeDirectory && !Directory.Exists(fullPath))
        {
            return null;
        }

        return fullPath;
    }

    private static bool IsUnderRoot(string path, string root)
    {
        var normalizedPath = Path.TrimEndingDirectorySeparator(Path.GetFullPath(path));
        var normalizedRoot = Path.TrimEndingDirectorySeparator(Path.GetFullPath(root));
        return normalizedPath.Equals(normalizedRoot, StringComparison.OrdinalIgnoreCase)
            || normalizedPath.StartsWith(
                normalizedRoot + Path.DirectorySeparatorChar,
                StringComparison.OrdinalIgnoreCase)
            || normalizedPath.StartsWith(
                normalizedRoot + Path.AltDirectorySeparatorChar,
                StringComparison.OrdinalIgnoreCase);
    }

    private static bool StartProcess(string fileName, string? arguments = null)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments ?? string.Empty,
                UseShellExecute = true
            };

            Process.Start(startInfo);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static IEnumerable<string> PathSegments(string path)
    {
        return path.Split(
            [Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar],
            StringSplitOptions.RemoveEmptyEntries);
    }
}

public sealed record FileSearchResult(
    string Path,
    string FileName,
    string RelativePath,
    string MatchType,
    string? Snippet,
    long SizeBytes,
    DateTime LastWriteTimeUtc);
