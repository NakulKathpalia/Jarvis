namespace Jarvis.Core.Agents.Coding.Search;

using Jarvis.Core.Agents.Coding.Models;

/// <summary>
/// Provides literal repository index search.
/// </summary>
public sealed class RepositorySearch
{
    /// <summary>
    /// Searches a repository index.
    /// </summary>
    /// <param name="index">The repository index.</param>
    /// <param name="request">The search request.</param>
    /// <returns>The search results.</returns>
    public SearchResult Search(RepositoryIndex index, SearchRequest request)
    {
        ArgumentNullException.ThrowIfNull(index);
        ArgumentNullException.ThrowIfNull(request);

        var comparison = request.Options.IgnoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        return new SearchResult
        {
            Files = index.Files.Files
                .Where(file => Matches(file.Name, request.FileName, comparison))
                .Where(file => Matches(file.Extension, NormalizeExtension(request.Extension), comparison))
                .Where(file => Matches(file.Language, request.Language, comparison))
                .ToList(),
            Folders = index.Folders
                .Where(folder => Matches(folder.RelativePath, request.Folder, comparison))
                .ToList(),
            Projects = index.Projects.Projects
                .Where(project => Matches(project.Name, request.Project, comparison))
                .ToList(),
            Configurations = index.Configurations.Configurations
                .Where(configuration => Matches(configuration.ConfigurationType, request.Configuration, comparison))
                .ToList(),
            Languages = index.Languages.FileCounts.Keys
                .Where(language => Matches(language, request.Language, comparison))
                .OrderBy(language => language, StringComparer.OrdinalIgnoreCase)
                .ToList()
        };
    }

    private static bool Matches(string value, string query, StringComparison comparison)
    {
        return string.IsNullOrWhiteSpace(query) || value.Contains(query, comparison);
    }

    private static string NormalizeExtension(string extension)
    {
        if (string.IsNullOrWhiteSpace(extension))
        {
            return string.Empty;
        }

        return extension.StartsWith('.') ? extension : "." + extension;
    }
}
