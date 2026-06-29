namespace Jarvis.Core.Agents.Coding.MultiAgent;

using Jarvis.Core.Agents.Coding.Autonomous;

/// <summary>
/// Produces security role results from review findings.
/// </summary>
public sealed class SecurityRole
{
    /// <summary>
    /// Executes the security role.
    /// </summary>
    public CodingAgentResult Execute(AutonomousCodingResult autonomousResult)
    {
        var review = autonomousResult.Iterations.LastOrDefault()?.ReviewResult;
        var securityFindings = review?.Findings
            .Where(finding => finding.Category.Equals("Security", StringComparison.OrdinalIgnoreCase))
            .ToList() ?? [];
        return new CodingAgentResult
        {
            Role = CodingAgentRole.Security,
            Succeeded = securityFindings.All(finding => finding.Severity < Review.Quality.ReviewSeverity.Error),
            Output = $"Security findings: {securityFindings.Count}."
        };
    }
}
