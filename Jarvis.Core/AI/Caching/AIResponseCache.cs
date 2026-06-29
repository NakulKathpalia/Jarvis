namespace Jarvis.Core.AI.Caching;

using System.Security.Cryptography;
using System.Text;
using Jarvis.Core.AI.Runtime;

/// <summary>
/// Provides a simple in-memory response cache.
/// </summary>
public sealed class AIResponseCache
{
    private readonly Dictionary<string, AIResponse> cache = new(StringComparer.Ordinal);
    private readonly object sync = new();

    /// <summary>
    /// Gets a cached response.
    /// </summary>
    public AIResponse? Get(AIRequest request, string model)
    {
        lock (sync)
        {
            return cache.TryGetValue(CreateKey(request, model), out var response) ? Clone(response, fromCache: true) : null;
        }
    }

    /// <summary>
    /// Stores a response in cache.
    /// </summary>
    public void Set(AIRequest request, string model, AIResponse response)
    {
        if (!response.Succeeded)
        {
            return;
        }

        lock (sync)
        {
            cache[CreateKey(request, model)] = Clone(response, fromCache: false);
        }
    }

    private static string CreateKey(AIRequest request, string model)
    {
        var options = $"{request.Options.Temperature}|{request.Options.TopP}|{request.Options.NumPredict}|{request.Options.NumContext}|{request.Options.Seed}";
        var text = $"{model}|{options}|{request.Prompt}";
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(text));
        return Convert.ToHexString(bytes);
    }

    private static AIResponse Clone(AIResponse response, bool fromCache)
    {
        var clone = new AIResponse
        {
            Succeeded = response.Succeeded,
            Provider = response.Provider,
            Model = response.Model,
            Text = response.Text,
            Duration = response.Duration,
            PromptSize = response.PromptSize,
            OriginalPromptSize = response.OriginalPromptSize,
            CompressedPromptSize = response.CompressedPromptSize,
            InputTokens = response.InputTokens,
            OutputTokens = response.OutputTokens,
            FinishReason = response.FinishReason,
            RawProviderResponse = response.RawProviderResponse,
            ProviderHealthStatus = response.ProviderHealthStatus,
            ModelHealthStatus = response.ModelHealthStatus,
            ErrorKind = response.ErrorKind,
            ErrorMessage = response.ErrorMessage,
            FromCache = fromCache
        };
        foreach (var warning in response.Warnings)
        {
            clone.Warnings.Add(warning);
        }

        return clone;
    }
}
