namespace Jarvis.Core.Agents.Coding.Workspace;

/// <summary>
/// Cleans managed runnable workspaces.
/// </summary>
public sealed class WorkspaceCleaner
{
    /// <summary>
    /// Deletes a workspace session when it is inside the managed runs folder.
    /// </summary>
    public bool Clean(WorkspaceSession session)
    {
        if (session.ApplyToRepository || !Directory.Exists(session.RootPath))
        {
            return false;
        }

        Directory.Delete(session.RootPath, recursive: true);
        return true;
    }
}
