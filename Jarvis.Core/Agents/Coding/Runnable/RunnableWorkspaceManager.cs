namespace Jarvis.Core.Agents.Coding.Runnable;

/// <summary>
/// Creates safe runnable workspaces outside the main source tree.
/// </summary>
public sealed class RunnableWorkspaceManager
{
    /// <summary>
    /// Creates a new runnable workspace.
    /// </summary>
    public RunnableWorkspace Create(string repositoryPath, bool applyToRepository)
    {
        var root = string.IsNullOrWhiteSpace(repositoryPath) ? Directory.GetCurrentDirectory() : repositoryPath;
        var runId = "ui-task-" + DateTime.Now.ToString("yyyyMMdd-HHmmss");
        var workspacePath = applyToRepository ? root : Path.Combine(root, ".jarvis-runs", runId);
        Directory.CreateDirectory(workspacePath);
        return new RunnableWorkspace
        {
            RootPath = workspacePath,
            RunId = runId
        };
    }
}
