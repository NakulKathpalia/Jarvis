namespace Jarvis.Core.Agents.Coding.FileGeneration;

using Jarvis.Core.Agents.Coding.Workspace;

/// <summary>
/// Writes generated projects to workspaces.
/// </summary>
public sealed class ProjectScaffolder
{
    /// <summary>
    /// Writes a generated project to the workspace.
    /// </summary>
    public IReadOnlyList<GeneratedFile> Scaffold(GeneratedProject project, WorkspaceSession workspace)
    {
        foreach (var file in project.Files)
        {
            var fullPath = Path.Combine(workspace.ProjectPath, file.RelativePath);
            var directory = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(fullPath, file.Content);
            file.FullPath = fullPath;
            workspace.Files.Add(new WorkspaceFile { RelativePath = file.RelativePath, FullPath = fullPath });
        }

        return project.Files;
    }
}
