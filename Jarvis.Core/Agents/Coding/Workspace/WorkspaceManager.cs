namespace Jarvis.Core.Agents.Coding.Workspace;

/// <summary>
/// Creates and tracks safe runnable workspaces.
/// </summary>
public sealed class WorkspaceManager
{
    private readonly WorkspaceHistory history;

    /// <summary>
    /// Initializes a new instance of the <see cref="WorkspaceManager"/> class.
    /// </summary>
    public WorkspaceManager(WorkspaceHistory? history = null)
    {
        this.history = history ?? new WorkspaceHistory();
    }

    /// <summary>
    /// Creates a workspace for a runnable execution.
    /// </summary>
    public WorkspaceSession Create(string repositoryPath, bool applyToRepository, WorkspaceTemplate? template = null)
    {
        var repo = string.IsNullOrWhiteSpace(repositoryPath) ? Directory.GetCurrentDirectory() : repositoryPath;
        template ??= new WorkspaceTemplate();
        var session = new WorkspaceSession { ApplyToRepository = applyToRepository };
        session.RootPath = applyToRepository
            ? repo
            : Path.Combine(repo, ".jarvis-runs", session.Id);
        session.ProjectPath = Path.Combine(session.RootPath, template.ProjectFolderName);
        Directory.CreateDirectory(session.ProjectPath);
        history.Add(session);
        return session;
    }
}
