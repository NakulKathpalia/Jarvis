namespace Jarvis.Core.Agents.Coding.MultiAgent;

using Jarvis.Core.Agents.Coding.Autonomous;

/// <summary>
/// Produces a review role result.
/// </summary>
public sealed class ReviewerRole
{
    /// <summary>
    /// Executes the reviewer role.
    /// </summary>
    public CodingAgentResult Execute(AutonomousCodingResult autonomousResult)
    {
        var review = autonomousResult.Iterations.LastOrDefault()?.ReviewResult;
        return new CodingAgentResult
        {
            Role = CodingAgentRole.Reviewer,
            Succeeded = review is not null && !review.BlocksApply,
            Output = review is null ? "No review result produced." : $"Findings: {review.Findings.Count}. Blocks apply: {review.BlocksApply}.",
            ReviewResult = review
        };
    }
}
