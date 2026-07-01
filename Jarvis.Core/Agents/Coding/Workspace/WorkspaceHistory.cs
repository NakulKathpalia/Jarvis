namespace Jarvis.Core.Agents.Coding.Workspace;

/// <summary>
/// Tracks managed workspace sessions in memory.
/// </summary>
public sealed class WorkspaceHistory
{
    private readonly List<WorkspaceSession> sessions = [];

    /// <summary>
    /// Adds a session to history.
    /// </summary>
    public void Add(WorkspaceSession session)
    {
        sessions.Add(session);
    }

    /// <summary>
    /// Gets all sessions.
    /// </summary>
    public IReadOnlyList<WorkspaceSession> All()
    {
        return sessions.ToList();
    }
}
