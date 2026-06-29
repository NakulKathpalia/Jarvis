namespace Jarvis.Core.AI.Providers;

using Jarvis.Core.AI.Health;
using Jarvis.Core.AI.Runtime;

/// <summary>
/// Defines a provider-independent LLM provider.
/// </summary>
public interface IAIProvider
{
    /// <summary>
    /// Gets the provider name.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets provider health.
    /// </summary>
    Task<ProviderHealth> GetHealthAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes an AI request.
    /// </summary>
    Task<AIResponse> GenerateAsync(AIRequest request, CancellationToken cancellationToken = default);
}
