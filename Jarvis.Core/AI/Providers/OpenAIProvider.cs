namespace Jarvis.Core.AI.Providers;

using Jarvis.Core.AI.Health;
using Jarvis.Core.AI.Runtime;

/// <summary>
/// Represents a future OpenAI provider integration.
/// </summary>
public sealed class OpenAIProvider : AIProviderBase
{
    /// <inheritdoc />
    public override string Name => "OpenAI";

    /// <inheritdoc />
    public override Task<ProviderHealth> GetHealthAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Unavailable("OpenAI provider is not configured in this local runtime."));
    }

    /// <inheritdoc />
    public override Task<AIResponse> GenerateAsync(AIRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new AIResponse
        {
            Succeeded = false,
            Provider = Name,
            Model = request.ModelName,
            ErrorKind = AIErrorKind.ProviderUnavailable,
            ErrorMessage = "OpenAI provider is a stub and is not configured."
        });
    }
}
