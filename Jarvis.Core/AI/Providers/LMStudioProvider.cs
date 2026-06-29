namespace Jarvis.Core.AI.Providers;

using Jarvis.Core.AI.Health;
using Jarvis.Core.AI.Runtime;

/// <summary>
/// Represents a future LM Studio provider integration.
/// </summary>
public sealed class LMStudioProvider : AIProviderBase
{
    /// <inheritdoc />
    public override string Name => "LMStudio";

    /// <inheritdoc />
    public override Task<ProviderHealth> GetHealthAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Unavailable("LM Studio provider is not configured in this local runtime."));
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
            ErrorMessage = "LM Studio provider is a stub and is not configured."
        });
    }
}
