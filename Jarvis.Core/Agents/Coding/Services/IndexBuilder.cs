namespace Jarvis.Core.Agents.Coding.Services;

using Jarvis.Core.Agents.Coding.Models;

/// <summary>
/// Builds repository indexes from repository reader output.
/// </summary>
public sealed class IndexBuilder
{
    /// <summary>
    /// Builds a repository index.
    /// </summary>
    /// <param name="readResult">The repository read result.</param>
    /// <returns>The repository index.</returns>
    public RepositoryIndex Build(RepositoryReadResult readResult)
    {
        ArgumentNullException.ThrowIfNull(readResult);

        return new RepositoryIndex
        {
            RootPath = readResult.RootPath,
            RepositoryName = readResult.RepositoryName,
            Files = new FileIndex { Files = readResult.Files },
            Projects = new ProjectIndex { Projects = readResult.Projects },
            Folders = readResult.Folders,
            Configurations = new ConfigurationIndex { Configurations = readResult.Configurations },
            Languages = new LanguageIndex
            {
                FileCounts = readResult.Files
                    .Where(file => !string.IsNullOrWhiteSpace(file.Language))
                    .GroupBy(file => file.Language, StringComparer.OrdinalIgnoreCase)
                    .OrderBy(group => group.Key, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(group => group.Key, group => group.Count(), StringComparer.OrdinalIgnoreCase)
            },
            Dependencies = readResult.Projects.SelectMany(project => project.ProjectReferences).ToList()
        };
    }
}
