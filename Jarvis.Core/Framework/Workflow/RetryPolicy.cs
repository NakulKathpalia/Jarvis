namespace Jarvis.Core.Framework.Workflow;

using Jarvis.Core.Framework.Models;

/// <summary>
/// Provides a simple retry policy for transient workflow step failures.
/// </summary>
public sealed class RetryPolicy : IRetryPolicy
{
    private static readonly string[] RetryableTerms =
    [
        "network",
        "temporary",
        "transient",
        "timeout",
        "timed out"
    ];

    private static readonly string[] NonRetryableTerms =
    [
        "validation",
        "permission",
        "unauthorized",
        "forbidden",
        "cancelled",
        "canceled"
    ];

    /// <inheritdoc />
    public int RetryCount { get; init; }

    /// <inheritdoc />
    public TimeSpan RetryDelay { get; init; } = TimeSpan.Zero;

    /// <inheritdoc />
    public bool UseExponentialBackoff { get; init; }

    /// <inheritdoc />
    public bool ShouldRetry(TaskResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (result.Succeeded)
        {
            return false;
        }

        var message = result.ErrorMessage ?? string.Empty;
        if (NonRetryableTerms.Any(term => message.Contains(term, StringComparison.OrdinalIgnoreCase)))
        {
            return false;
        }

        return RetryableTerms.Any(term => message.Contains(term, StringComparison.OrdinalIgnoreCase));
    }
}
