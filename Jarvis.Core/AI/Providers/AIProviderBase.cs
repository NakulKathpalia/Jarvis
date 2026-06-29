namespace Jarvis.Core.AI.Providers;

using Jarvis.Core.AI.Health;
using Jarvis.Core.AI.Runtime;

/// <summary>
/// Provides common behavior for AI providers.
/// </summary>
public abstract class AIProviderBase : IAIProvider
{
    /// <inheritdoc />
    public abstract string Name { get; }

    /// <inheritdoc />
    public abstract Task<ProviderHealth> GetHealthAsync(CancellationToken cancellationToken = default);

    /// <inheritdoc />
    public abstract Task<AIResponse> GenerateAsync(AIRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates an unavailable health response.
    /// </summary>
    protected ProviderHealth Unavailable(string reason)
    {
        return new ProviderHealth
        {
            Provider = Name,
            Available = false,
            Status = "Unavailable",
            FailureReason = reason
        };
    }
}
