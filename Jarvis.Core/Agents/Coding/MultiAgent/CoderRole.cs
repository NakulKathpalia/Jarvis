namespace Jarvis.Core.Agents.Coding.MultiAgent;

using Jarvis.Core.Agents.Coding.Autonomous;

/// <summary>
/// Produces a coding proposal role result.
/// </summary>
public sealed class CoderRole
{
    /// <summary>
    /// Executes the coder role.
    /// </summary>
    public CodingAgentResult Execute(AutonomousCodingResult autonomousResult)
    {
        return new CodingAgentResult
        {
            Role = CodingAgentRole.Coder,
            Succeeded = autonomousResult.Succeeded,
            Output = autonomousResult.FinalReport,
            AutonomousResult = autonomousResult
        };
    }
}
