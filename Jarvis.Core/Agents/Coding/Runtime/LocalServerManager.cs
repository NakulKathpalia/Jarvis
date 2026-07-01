namespace Jarvis.Core.Agents.Coding.Runtime;

using Jarvis.Core.Agents.Coding.Runnable;

/// <summary>
/// Coordinates port selection and local dev server startup.
/// </summary>
public sealed class LocalServerManager
{
    private readonly PortFinder portFinder;
    private readonly DevServerLauncher launcher;

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalServerManager"/> class.
    /// </summary>
    public LocalServerManager(PortFinder? portFinder = null, DevServerLauncher? launcher = null)
    {
        this.portFinder = portFinder ?? new PortFinder();
        this.launcher = launcher ?? new DevServerLauncher();
    }

    /// <summary>
    /// Starts a local server.
    /// </summary>
    public RunningProject Start(string projectPath, RunnableTaskType taskType)
    {
        var port = portFinder.Find(5173);
        return launcher.Launch(projectPath, taskType, port);
    }
}
