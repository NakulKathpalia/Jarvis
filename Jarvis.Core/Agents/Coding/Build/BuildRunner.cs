namespace Jarvis.Core.Agents.Coding.Build;

using System.Diagnostics;
using Jarvis.Core.Agents.Coding.Models;

/// <summary>
/// Runs build commands through local CLI tools.
/// </summary>
public sealed class BuildRunner
{
    /// <summary>
    /// Runs a build command.
    /// </summary>
    public async Task<(int ExitCode, string Output, TimeSpan Duration)> RunAsync(
        BuildConfiguration configuration,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var start = DateTimeOffset.UtcNow;
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = configuration.Command,
                Arguments = configuration.Arguments,
                WorkingDirectory = configuration.WorkingDirectory,
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
        return (process.ExitCode, output, DateTimeOffset.UtcNow - start);
    }
}
