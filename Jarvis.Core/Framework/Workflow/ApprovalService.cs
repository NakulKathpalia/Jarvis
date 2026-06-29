namespace Jarvis.Core.Framework.Workflow;

/// <summary>
/// Provides an in-memory approval service.
/// </summary>
public sealed class ApprovalService : IApprovalService
{
    private readonly Dictionary<string, TaskCompletionSource<ApprovalResult>> pending = [];
    private readonly object gate = new();

    /// <inheritdoc />
    public async Task<ApprovalResult> RequestApprovalAsync(
        ApprovalRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var source = new TaskCompletionSource<ApprovalResult>(TaskCreationOptions.RunContinuationsAsynchronously);
        lock (gate)
        {
            pending[request.RequestId] = source;
        }

        using var registration = cancellationToken.Register(() => source.TrySetCanceled(cancellationToken));
        try
        {
            return await source.Task;
        }
        finally
        {
            lock (gate)
            {
                pending.Remove(request.RequestId);
            }
        }
    }

    /// <inheritdoc />
    public bool Complete(ApprovalResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        lock (gate)
        {
            return pending.TryGetValue(result.RequestId, out var source) &&
                source.TrySetResult(result);
        }
    }
}
