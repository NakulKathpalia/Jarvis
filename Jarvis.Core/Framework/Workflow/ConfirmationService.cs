namespace Jarvis.Core.Framework.Workflow;

/// <summary>
/// Provides an in-memory confirmation service.
/// </summary>
public sealed class ConfirmationService : IConfirmationService
{
    private readonly Dictionary<string, TaskCompletionSource<ConfirmationResult>> pending = [];
    private readonly object gate = new();

    /// <inheritdoc />
    public async Task<ConfirmationResult> RequestConfirmationAsync(
        ConfirmationRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var source = new TaskCompletionSource<ConfirmationResult>(TaskCreationOptions.RunContinuationsAsynchronously);
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
    public bool Complete(ConfirmationResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        lock (gate)
        {
            return pending.TryGetValue(result.RequestId, out var source) &&
                source.TrySetResult(result);
        }
    }
}
