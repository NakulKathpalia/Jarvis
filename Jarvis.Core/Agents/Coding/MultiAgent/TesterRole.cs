namespace Jarvis.Core.Agents.Coding.MultiAgent;

using Jarvis.Core.Agents.Coding.Build;
using Jarvis.Core.Agents.Coding.Models;

/// <summary>
/// Runs build validation for the tester role.
/// </summary>
public sealed class TesterRole
{
    private readonly BuildEngine buildEngine;

    /// <summary>
    /// Initializes a new instance of the <see cref="TesterRole"/> class.
    /// </summary>
    public TesterRole(BuildEngine? buildEngine = null)
    {
        this.buildEngine = buildEngine ?? new BuildEngine();
    }

    /// <summary>
    /// Executes the tester role.
    /// </summary>
    public async Task<CodingAgentResult> ExecuteAsync(string repositoryPath, CancellationToken cancellationToken = default)
    {
        var build = await buildEngine.BuildAsync(new BuildRequest { RepositoryPath = repositoryPath }, cancellationToken);
        return new CodingAgentResult
        {
            Role = CodingAgentRole.Tester,
            Succeeded = build.Succeeded,
            Output = $"Build succeeded: {build.Succeeded}. Errors: {build.Errors.Count}. Warnings: {build.Warnings.Count}.",
            BuildResult = build
        };
    }
}
