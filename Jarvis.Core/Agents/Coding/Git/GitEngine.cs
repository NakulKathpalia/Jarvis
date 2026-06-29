namespace Jarvis.Core.Agents.Coding.Git;

using Jarvis.Core.Agents.Coding.Models;

/// <summary>
/// Provides local Git repository operations through the Git CLI.
/// </summary>
public sealed class GitEngine
{
    private readonly GitRunner runner;

    /// <summary>
    /// Initializes a new instance of the <see cref="GitEngine"/> class.
    /// </summary>
    public GitEngine()
        : this(new GitRunner())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GitEngine"/> class.
    /// </summary>
    public GitEngine(GitRunner runner)
    {
        this.runner = runner;
    }

    /// <summary>
    /// Reads Git status.
    /// </summary>
    public async Task<GitStatus> StatusAsync(GitRepository repository, CancellationToken cancellationToken = default)
    {
        var result = await runner.RunAsync(repository.Path, "status --short", cancellationToken);
        return new GitStatus { Lines = SplitLines(result.Output) };
    }

    /// <summary>
    /// Reads Git diff.
    /// </summary>
    public async Task<GitDiff> DiffAsync(GitRepository repository, CancellationToken cancellationToken = default)
    {
        var result = await runner.RunAsync(repository.Path, "diff -- .", cancellationToken);
        return new GitDiff { Text = result.Output };
    }

    /// <summary>
    /// Reads branch information.
    /// </summary>
    public async Task<GitBranch> BranchAsync(GitRepository repository, CancellationToken cancellationToken = default)
    {
        var result = await runner.RunAsync(repository.Path, "branch --show-current", cancellationToken);
        var branches = await runner.RunAsync(repository.Path, "branch --list", cancellationToken);
        return new GitBranch
        {
            Current = result.Output.Trim(),
            Branches = SplitLines(branches.Output).Select(line => line.Trim().TrimStart('*').Trim()).ToList()
        };
    }

    /// <summary>
    /// Checks out a branch.
    /// </summary>
    public Task<GitOperationResult> CheckoutAsync(GitRepository repository, string branch, CancellationToken cancellationToken = default)
    {
        return runner.RunAsync(repository.Path, $"checkout {branch}", cancellationToken);
    }

    /// <summary>
    /// Creates a commit.
    /// </summary>
    public Task<GitOperationResult> CommitAsync(GitRepository repository, string message, CancellationToken cancellationToken = default)
    {
        return runner.RunAsync(repository.Path, $"commit -m \"{message.Replace("\"", "\\\"")}\"", cancellationToken);
    }

    /// <summary>
    /// Pulls from the configured remote.
    /// </summary>
    public Task<GitOperationResult> PullAsync(GitRepository repository, CancellationToken cancellationToken = default)
    {
        return runner.RunAsync(repository.Path, "pull", cancellationToken);
    }

    /// <summary>
    /// Pushes to the configured remote.
    /// </summary>
    public Task<GitOperationResult> PushAsync(GitRepository repository, CancellationToken cancellationToken = default)
    {
        return runner.RunAsync(repository.Path, "push", cancellationToken);
    }

    /// <summary>
    /// Reads commit history.
    /// </summary>
    public async Task<GitHistory> LogAsync(GitRepository repository, int count = 10, CancellationToken cancellationToken = default)
    {
        var result = await runner.RunAsync(repository.Path, $"log -{count} --pretty=format:%H|%s", cancellationToken);
        return new GitHistory
        {
            Commits = SplitLines(result.Output)
                .Select(ParseCommit)
                .Where(commit => !string.IsNullOrWhiteSpace(commit.Hash))
                .ToList()
        };
    }

    /// <summary>
    /// Creates a Git tag.
    /// </summary>
    public Task<GitOperationResult> TagAsync(GitRepository repository, string tag, CancellationToken cancellationToken = default)
    {
        return runner.RunAsync(repository.Path, $"tag {tag}", cancellationToken);
    }

    private static List<string> SplitLines(string text)
    {
        return text.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries).ToList();
    }

    private static GitCommit ParseCommit(string line)
    {
        var separator = line.IndexOf('|', StringComparison.Ordinal);
        return separator < 0
            ? new GitCommit()
            : new GitCommit { Hash = line[..separator], Subject = line[(separator + 1)..] };
    }
}
