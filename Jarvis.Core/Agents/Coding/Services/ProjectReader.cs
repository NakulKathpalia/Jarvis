namespace Jarvis.Core.Agents.Coding.Services;

using Jarvis.Core.Agents.Coding.Models;

/// <summary>
/// Reads solution and project files discovered in a repository.
/// </summary>
public sealed class ProjectReader
{
    /// <summary>
    /// Reads project files from scanned repository files.
    /// </summary>
    /// <param name="files">The scanned files.</param>
    /// <returns>The discovered projects.</returns>
    public IReadOnlyList<RepositoryProject> Read(IEnumerable<RepositoryFile> files)
    {
        return files
            .Where(file => IsProjectFile(file.Extension))
            .Select(file => new RepositoryProject
            {
                Path = file.Path,
                RelativePath = file.RelativePath,
                Name = Path.GetFileNameWithoutExtension(file.Name),
                ProjectType = DetectProjectType(file.Extension)
            })
            .OrderBy(project => project.RelativePath, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static bool IsProjectFile(string extension)
    {
        return extension.Equals(".sln", StringComparison.OrdinalIgnoreCase) ||
            extension.Equals(".csproj", StringComparison.OrdinalIgnoreCase) ||
            extension.Equals(".fsproj", StringComparison.OrdinalIgnoreCase) ||
            extension.Equals(".vbproj", StringComparison.OrdinalIgnoreCase);
    }

    private static string DetectProjectType(string extension)
    {
        return extension.ToLowerInvariant() switch
        {
            ".sln" => "Solution",
            ".csproj" => "C# Project",
            ".fsproj" => "F# Project",
            ".vbproj" => "VB Project",
            _ => "Project"
        };
    }
}
