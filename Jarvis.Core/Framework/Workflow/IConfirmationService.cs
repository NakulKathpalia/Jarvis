namespace Jarvis.Core.Framework.Workflow;

/// <summary>
/// Defines a framework service for workflow confirmations.
/// </summary>
public interface IConfirmationService
{
    /// <summary>
    /// Requests confirmation and waits for a result.
    /// </summary>
    /// <param name="request">The confirmation request.</param>
    /// <param name="cancellationToken">A token that cancels waiting.</param>
    /// <returns>The confirmation result.</returns>
    Task<ConfirmationResult> RequestConfirmationAsync(
        ConfirmationRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Completes a pending confirmation request.
    /// </summary>
    /// <param name="result">The confirmation result.</param>
    /// <returns><c>true</c> when a pending request was completed.</returns>
    bool Complete(ConfirmationResult result);
}
