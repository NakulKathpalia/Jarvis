namespace Jarvis.Core.Agents.Coding.Git;

using System.Diagnostics;
using Jarvis.Core.Agents.Coding.Models;

/// <summary>
/// Runs local Git CLI commands.
/// </summary>
public sealed class GitRunner
{
    /// <summary>
    /// Runs a Git command.
    /// </summary>
    public async Task<GitOperationResult> RunAsync(
        string repositoryPath,
        string arguments,
        CancellationToken cancellationToken = default)
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = arguments,
                WorkingDirectory = repositoryPath,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();
        var stdout = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var stderr = process.StandardError.ReadToEndAsync(cancellationToken);
        await process.WaitForExitAsync(cancellationToken);
        var output = string.Join(Environment.NewLine, await stdout, await stderr);
        return new GitOperationResult
        {
            Succeeded = process.ExitCode == 0,
            ExitCode = process.ExitCode,
            Output = output.Trim()
        };
    }
}
