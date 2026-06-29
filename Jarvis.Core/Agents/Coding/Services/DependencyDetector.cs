namespace Jarvis.Core.Agents.Coding.Services;

using System.Xml.Linq;
using Jarvis.Core.Agents.Coding.Models;

/// <summary>
/// Detects project reference dependencies.
/// </summary>
public sealed class DependencyDetector
{
    /// <summary>
    /// Reads project references from discovered project files.
    /// </summary>
    /// <param name="projects">The discovered projects.</param>
    public void DetectProjectReferences(IEnumerable<RepositoryProject> projects)
    {
        foreach (var project in projects.Where(project => project.Path.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase)))
        {
            project.ProjectReferences = ReadProjectReferences(project).ToList();
        }
    }

    private static IEnumerable<RepositoryDependency> ReadProjectReferences(RepositoryProject project)
    {
        XDocument document;
        try
        {
            document = XDocument.Load(project.Path);
        }
        catch
        {
            yield break;
        }

        foreach (var element in document.Descendants().Where(node => node.Name.LocalName == "ProjectReference"))
        {
            var include = element.Attribute("Include")?.Value;
            if (string.IsNullOrWhiteSpace(include))
            {
                continue;
            }

            yield return new RepositoryDependency
            {
                SourceProject = project.Name,
                ReferencePath = include
            };
        }
    }
}
