namespace Jarvis.Core.Agents.Coding.Runnable;

using System.Diagnostics;

/// <summary>
/// Starts local development servers for runnable workspaces.
/// </summary>
public sealed class LocalDevServer
{
    /// <summary>
    /// Starts a local server for the workspace.
    /// </summary>
    public RunnableResult Start(RunnableWorkspace workspace, RunnableTaskType taskType, int port)
    {
        return taskType == RunnableTaskType.React
            ? StartReact(workspace, port)
            : StartStatic(workspace, port);
    }

    private static RunnableResult StartStatic(RunnableWorkspace workspace, int port)
    {
        foreach (var command in new[] { "python", "py", "python3" })
        {
            var result = TryStart(workspace, command, $"-m http.server {port} --bind 127.0.0.1", port);
            if (result.Succeeded)
            {
                return result;
            }
        }

        return Failure(workspace, port, "Python was not found. Install Python or open index.html directly.");
    }

    private static RunnableResult StartReact(RunnableWorkspace workspace, int port)
    {
        if (!Directory.Exists(Path.Combine(workspace.RootPath, "node_modules")))
        {
            var install = RunOnce(workspace.RootPath, "npm", "install");
            if (install != 0)
            {
                return Failure(workspace, port, "npm install failed. Check Node.js and network/package availability.");
            }
        }

        return TryStart(workspace, "npm", $"run dev -- --port {port}", port);
    }

    private static RunnableResult TryStart(RunnableWorkspace workspace, string command, string arguments, int port)
    {
        try
        {
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = command,
                Arguments = arguments,
                WorkingDirectory = workspace.RootPath,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = false,
                RedirectStandardOutput = false
            });

            if (process is null)
            {
                return Failure(workspace, port, $"Failed to start {command}.");
            }

            File.WriteAllText(Path.Combine(workspace.RootPath, ".jarvis-server.pid"), process.Id.ToString());
            return new RunnableResult
            {
                Succeeded = true,
                WorkspacePath = workspace.RootPath,
                Port = port,
                Url = $"http://localhost:{port}/",
                ServerStatus = "Running",
                ProcessId = process.Id,
                StopInstructions = $"Stop process {process.Id}, or run: taskkill /PID {process.Id} /F"
            };
        }
        catch (Exception ex)
        {
            return Failure(workspace, port, $"{command} failed to start: {ex.Message}");
        }
    }

    private static int RunOnce(string workingDirectory, string command, string arguments)
    {
        try
        {
            using var process = Process.Start(new ProcessStartInfo
            {
                FileName = command,
                Arguments = arguments,
                WorkingDirectory = workingDirectory,
                UseShellExecute = false,
                CreateNoWindow = true
            });
            process?.WaitForExit();
            return process?.ExitCode ?? 1;
        }
        catch
        {
            return 1;
        }
    }

    private static RunnableResult Failure(RunnableWorkspace workspace, int port, string error)
    {
        var result = new RunnableResult
        {
            Succeeded = false,
            WorkspacePath = workspace.RootPath,
            Port = port,
            ServerStatus = "Failed",
            StopInstructions = "No server process was started."
        };
        result.Errors.Add(error);
        return result;
    }
}
