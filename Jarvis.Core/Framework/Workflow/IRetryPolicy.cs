namespace Jarvis.Core.Framework.Workflow;

using Jarvis.Core.Framework.Models;

/// <summary>
/// Defines whether a failed workflow step should be retried.
/// </summary>
public interface IRetryPolicy
{
    /// <summary>
    /// Gets the maximum retry count.
    /// </summary>
    int RetryCount { get; }

    /// <summary>
    /// Gets the delay before the first retry.
    /// </summary>
    TimeSpan RetryDelay { get; }

    /// <summary>
    /// Gets a value indicating whether retry delays should use exponential backoff.
    /// </summary>
    bool UseExponentialBackoff { get; }

    /// <summary>
    /// Determines whether a step result is retryable.
    /// </summary>
    /// <param name="result">The failed task result.</param>
    /// <returns><c>true</c> when the failure can be retried.</returns>
    bool ShouldRetry(TaskResult result);
}
