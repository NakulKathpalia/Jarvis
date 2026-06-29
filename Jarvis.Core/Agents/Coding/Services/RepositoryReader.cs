namespace Jarvis.Core.Agents.Coding.Services;

using Jarvis.Core.Agents.Coding.Models;

/// <summary>
/// Loads repository structure and project metadata.
/// </summary>
public sealed class RepositoryReader
{
    private readonly FolderScanner folderScanner;
    private readonly FileScanner fileScanner;
    private readonly ProjectReader projectReader;
    private readonly ConfigurationDetector configurationDetector;
    private readonly DependencyDetector dependencyDetector;

    /// <summary>
    /// Initializes a new instance of the <see cref="RepositoryReader"/> class.
    /// </summary>
    public RepositoryReader()
        : this(new FolderScanner(), new FileScanner(), new ProjectReader(), new ConfigurationDetector(), new DependencyDetector())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RepositoryReader"/> class.
    /// </summary>
    public RepositoryReader(
        FolderScanner folderScanner,
        FileScanner fileScanner,
        ProjectReader projectReader,
        ConfigurationDetector configurationDetector,
        DependencyDetector dependencyDetector)
    {
        this.folderScanner = folderScanner;
        this.fileScanner = fileScanner;
        this.projectReader = projectReader;
        this.configurationDetector = configurationDetector;
        this.dependencyDetector = dependencyDetector;
    }

    /// <summary>
    /// Reads a repository from the supplied path.
    /// </summary>
    /// <param name="path">The repository path.</param>
    /// <returns>The repository read result.</returns>
    public RepositoryReadResult Read(string path)
    {
        var rootPath = DetectRoot(path);
        var folders = folderScanner.Scan(rootPath).ToList();
        var files = fileScanner.Scan(rootPath).ToList();
        var projects = projectReader.Read(files).ToList();
        dependencyDetector.DetectProjectReferences(projects);

        return new RepositoryReadResult
        {
            RootPath = rootPath,
            RepositoryName = new DirectoryInfo(rootPath).Name,
            Folders = folders,
            Files = files,
            Projects = projects,
            Configurations = configurationDetector.Detect(files).ToList()
        };
    }

    /// <summary>
    /// Detects the repository root from a supplied file or directory path.
    /// </summary>
    /// <param name="path">The input path.</param>
    /// <returns>The detected repository root.</returns>
    public string DetectRoot(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        var fullPath = Path.GetFullPath(path);
        var directory = File.Exists(fullPath)
            ? new FileInfo(fullPath).Directory
            : new DirectoryInfo(fullPath);

        if (directory is null || !directory.Exists)
        {
            throw new DirectoryNotFoundException($"Repository path was not found: {path}");
        }

        var current = directory;
        while (current is not null)
        {
            if (Directory.Exists(Path.Combine(current.FullName, ".git")) ||
                Directory.EnumerateFiles(current.FullName, "*.sln").Any())
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        return directory.FullName;
    }
}
