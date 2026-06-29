namespace Jarvis.Core.Agents.Coding.MultiAgent;

using Jarvis.Core.Agents.Coding.AI;
using Jarvis.Core.Agents.Coding.Autonomous;

/// <summary>
/// Coordinates sequential coding roles.
/// </summary>
public sealed class CodingOrchestrator
{
    private readonly AutonomousCodingEngine autonomousEngine;
    private readonly PlannerRole plannerRole = new();
    private readonly CoderRole coderRole = new();
    private readonly ReviewerRole reviewerRole = new();
    private readonly TesterRole testerRole = new();
    private readonly SecurityRole securityRole = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="CodingOrchestrator"/> class.
    /// </summary>
    public CodingOrchestrator(ICodingModelClient modelClient)
    {
        autonomousEngine = new AutonomousCodingEngine(modelClient);
    }

    /// <summary>
    /// Executes the sequential multi-agent preview flow.
    /// </summary>
    public async Task<IReadOnlyList<CodingAgentResult>> RunPreviewAsync(
        string repositoryPath,
        string userRequest,
        string modelName = "",
        CancellationToken cancellationToken = default)
    {
        var autonomous = await autonomousEngine.RunAsync(new AutonomousCodingRequest
        {
            RepositoryPath = repositoryPath,
            UserRequest = userRequest,
            ModelName = modelName,
            Mode = CodingExecutionMode.PreviewOnly
        }, cancellationToken);
        var results = new List<CodingAgentResult>
        {
            plannerRole.Execute(autonomous.ContextResult),
            coderRole.Execute(autonomous),
            reviewerRole.Execute(autonomous),
            securityRole.Execute(autonomous),
            await testerRole.ExecuteAsync(repositoryPath, cancellationToken)
        };
        return results;
    }
}
