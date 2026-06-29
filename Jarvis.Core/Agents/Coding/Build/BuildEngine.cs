namespace Jarvis.Core.Agents.Coding.Build;

using Jarvis.Core.Agents.Coding.Models;

/// <summary>
/// Detects and runs repository builds.
/// </summary>
public sealed class BuildEngine
{
    private readonly BuildRunner runner;
    private readonly ErrorAnalyzer analyzer;

    /// <summary>
    /// Initializes a new instance of the <see cref="BuildEngine"/> class.
    /// </summary>
    public BuildEngine()
        : this(new BuildRunner(), new ErrorAnalyzer())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BuildEngine"/> class.
    /// </summary>
    public BuildEngine(BuildRunner runner, ErrorAnalyzer analyzer)
    {
        this.runner = runner;
        this.analyzer = analyzer;
    }

    /// <summary>
    /// Runs a build.
    /// </summary>
    public async Task<BuildResult> BuildAsync(BuildRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var configuration = request.Configuration ?? Detect(request.RepositoryPath, request.NoRestore);
        var run = await runner.RunAsync(configuration, cancellationToken);
        var errors = analyzer.ParseErrors(run.Output).ToList();
        var warnings = analyzer.ParseWarnings(run.Output).ToList();
        return new BuildResult
        {
            Succeeded = run.ExitCode == 0,
            Configuration = configuration,
            Output = run.Output,
            Errors = errors,
            Warnings = warnings,
            Statistics = new BuildStatistics
            {
                Duration = run.Duration,
                ExitCode = run.ExitCode,
                ErrorCount = errors.Count,
                WarningCount = warnings.Count
            }
        };
    }

    /// <summary>
    /// Detects a build configuration for a repository.
    /// </summary>
    public BuildConfiguration Detect(string repositoryPath, bool noRestore = false)
    {
        var root = string.IsNullOrWhiteSpace(repositoryPath) ? Directory.GetCurrentDirectory() : repositoryPath;
        if (Directory.EnumerateFiles(root, "*.sln").Any() || Directory.EnumerateFiles(root, "*.csproj").Any())
        {
            return new BuildConfiguration
            {
                Tool = "dotnet",
                Command = "dotnet",
                Arguments = noRestore ? "build -c Release --no-restore" : "build -c Release",
                WorkingDirectory = root
            };
        }

        if (File.Exists(Path.Combine(root, "pnpm-lock.yaml")))
        {
            return Create(root, "pnpm", "pnpm", "run build");
        }

        if (File.Exists(Path.Combine(root, "yarn.lock")))
        {
            return Create(root, "yarn", "yarn", "build");
        }

        if (File.Exists(Path.Combine(root, "package.json")))
        {
            return Create(root, "npm", "npm", "run build");
        }

        if (File.Exists(Path.Combine(root, "Cargo.toml")))
        {
            return Create(root, "cargo", "cargo", "build");
        }

        if (File.Exists(Path.Combine(root, "pom.xml")))
        {
            return Create(root, "maven", "mvn", "test");
        }

        if (File.Exists(Path.Combine(root, "build.gradle")))
        {
            return Create(root, "gradle", "gradle", "build");
        }

        if (File.Exists(Path.Combine(root, "requirements.txt")))
        {
            return Create(root, "python", "python", "-m compileall .");
        }

        return Create(root, "unknown", "dotnet", "build -c Release");
    }

    private static BuildConfiguration Create(string root, string tool, string command, string arguments)
    {
        return new BuildConfiguration
        {
            Tool = tool,
            Command = command,
            Arguments = arguments,
            WorkingDirectory = root
        };
    }
}
