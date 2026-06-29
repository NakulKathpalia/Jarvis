namespace Jarvis.Core.Framework.Workflow;

/// <summary>
/// Defines a framework service for sensitive workflow approvals.
/// </summary>
public interface IApprovalService
{
    /// <summary>
    /// Requests approval and waits for a result.
    /// </summary>
    /// <param name="request">The approval request.</param>
    /// <param name="cancellationToken">A token that cancels waiting.</param>
    /// <returns>The approval result.</returns>
    Task<ApprovalResult> RequestApprovalAsync(
        ApprovalRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Completes a pending approval request.
    /// </summary>
    /// <param name="result">The approval result.</param>
    /// <returns><c>true</c> when a pending request was completed.</returns>
    bool Complete(ApprovalResult result);
}
