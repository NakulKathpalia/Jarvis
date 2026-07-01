namespace Jarvis.Core.Agents.Coding.Runtime;

using System.Diagnostics;
using Jarvis.Core.Agents.Coding.Runnable;

/// <summary>
/// Launches local development servers.
/// </summary>
public sealed class DevServerLauncher
{
    /// <summary>
    /// Launches a server for the project.
    /// </summary>
    public RunningProject Launch(string projectPath, RunnableTaskType taskType, int port)
    {
        return taskType is RunnableTaskType.React
            ? LaunchReact(projectPath, port)
            : LaunchStatic(projectPath, port);
    }

    private static RunningProject LaunchStatic(string projectPath, int port)
    {
        foreach (var command in new[] { "python", "py", "python3" })
        {
            var result = TryStart(projectPath, command, $"-m http.server {port} --bind 127.0.0.1", port);
            if (result.IsRunning)
            {
                return result;
            }
        }

        return Failure(port, "Python was not found. Install Python or open index.html directly.");
    }

    private static RunningProject LaunchReact(string projectPath, int port)
    {
        if (!Directory.Exists(Path.Combine(projectPath, "node_modules")))
        {
            var install = RunOnce(projectPath, "npm", "install");
            if (install != 0)
            {
                return Failure(port, "npm install failed. Check Node.js and package access.");
            }
        }

        return TryStart(projectPath, "npm", $"run dev -- --port {port}", port);
    }

    private static RunningProject TryStart(string workingDirectory, string command, string arguments, int port)
    {
        try
        {
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = command,
                Arguments = arguments,
                WorkingDirectory = workingDirectory,
                UseShellExecute = false,
                CreateNoWindow = true
            });
            if (process is null)
            {
                return Failure(port, $"Failed to start {command}.");
            }

            File.WriteAllText(Path.Combine(workingDirectory, ".jarvis-server.pid"), process.Id.ToString());
            return new RunningProject
            {
                IsRunning = true,
                Port = port,
                Url = $"http://localhost:{port}/",
                ProcessId = process.Id,
                Status = "Running",
                StopCommand = $"taskkill /PID {process.Id} /F"
            };
        }
        catch (Exception ex)
        {
            return Failure(port, $"{command} failed to start: {ex.Message}");
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

    private static RunningProject Failure(int port, string message)
    {
        var project = new RunningProject
        {
            IsRunning = false,
            Port = port,
            Status = "Failed",
            StopCommand = "No server process was started."
        };
        project.Logs.Add(message);
        return project;
    }
}
