namespace Jarvis.Core.Framework.Workflow;

using Jarvis.Core.Framework.Models;

/// <summary>
/// Executes retryable workflow work according to a retry policy.
/// </summary>
public sealed class RetryExecutor
{
    /// <summary>
    /// Executes an operation and retries transient failures.
    /// </summary>
    /// <param name="operation">The operation to execute.</param>
    /// <param name="retryPolicy">The retry policy.</param>
    /// <param name="onRetryAsync">An optional callback invoked before each retry.</param>
    /// <param name="cancellationToken">A token that cancels execution.</param>
    /// <returns>The final task result.</returns>
    public async Task<TaskResult> ExecuteAsync(
        Func<CancellationToken, Task<TaskResult>> operation,
        IRetryPolicy? retryPolicy,
        Func<int, TaskResult, CancellationToken, Task>? onRetryAsync = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);

        var attempt = 0;
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var result = await operation(cancellationToken);

            if (retryPolicy is null || attempt >= retryPolicy.RetryCount || !retryPolicy.ShouldRetry(result))
            {
                return result;
            }

            attempt++;
            if (onRetryAsync is not null)
            {
                await onRetryAsync(attempt, result, cancellationToken);
            }

            var delay = CalculateDelay(retryPolicy, attempt);
            if (delay > TimeSpan.Zero)
            {
                await Task.Delay(delay, cancellationToken);
            }
        }
    }

    private static TimeSpan CalculateDelay(IRetryPolicy retryPolicy, int attempt)
    {
        if (!retryPolicy.UseExponentialBackoff || retryPolicy.RetryDelay <= TimeSpan.Zero)
        {
            return retryPolicy.RetryDelay;
        }

        var multiplier = Math.Pow(2, attempt - 1);
        return TimeSpan.FromMilliseconds(retryPolicy.RetryDelay.TotalMilliseconds * multiplier);
    }
}
