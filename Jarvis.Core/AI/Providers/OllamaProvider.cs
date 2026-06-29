namespace Jarvis.Core.AI.Providers;

using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;
using Jarvis.Core.AI.Health;
using Jarvis.Core.AI.Runtime;

/// <summary>
/// Provides AI execution through a local Ollama server.
/// </summary>
public sealed class OllamaProvider : AIProviderBase
{
    private readonly HttpClient httpClient;
    private readonly string baseUrl;

    /// <summary>
    /// Initializes a new instance of the <see cref="OllamaProvider"/> class.
    /// </summary>
    public OllamaProvider(HttpClient httpClient, string baseUrl)
    {
        this.httpClient = httpClient;
        this.baseUrl = string.IsNullOrWhiteSpace(baseUrl) ? "http://localhost:11434" : baseUrl.TrimEnd('/');
    }

    /// <inheritdoc />
    public override string Name => "Ollama";

    /// <inheritdoc />
    public override async Task<ProviderHealth> GetHealthAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await httpClient.GetAsync($"{baseUrl}/api/tags", cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return Unavailable($"Ollama returned HTTP {(int)response.StatusCode}.");
            }

            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            using var document = JsonDocument.Parse(body);
            var health = new ProviderHealth
            {
                Provider = Name,
                Available = true,
                Status = "Available"
            };

            if (document.RootElement.TryGetProperty("models", out var models) &&
                models.ValueKind == JsonValueKind.Array)
            {
                foreach (var model in models.EnumerateArray())
                {
                    if (model.TryGetProperty("name", out var name) &&
                        !string.IsNullOrWhiteSpace(name.GetString()))
                    {
                        health.AvailableModels.Add(name.GetString()!);
                    }
                }
            }

            return health;
        }
        catch (TaskCanceledException ex)
        {
            return Unavailable($"Ollama health check timed out: {ex.Message}");
        }
        catch (HttpRequestException ex)
        {
            return Unavailable($"Ollama is unavailable: {ex.Message}");
        }
        catch (Exception ex)
        {
            return Unavailable($"Ollama health check failed: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public override async Task<AIResponse> GenerateAsync(AIRequest request, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        using var timeoutSource = CreateTimeoutSource(request, cancellationToken);
        try
        {
            using var response = await httpClient.PostAsJsonAsync(
                $"{baseUrl}/api/generate",
                new
                {
                    model = request.ModelName,
                    prompt = request.Prompt,
                    stream = request.Options.Streaming,
                    options = BuildOptions(request.Options)
                },
                timeoutSource.Token);

            var body = await response.Content.ReadAsStringAsync(timeoutSource.Token);
            stopwatch.Stop();
            if (!response.IsSuccessStatusCode)
            {
                return Failure(request, stopwatch.Elapsed, AIErrorKind.ProviderError, body, body);
            }

            using var document = JsonDocument.Parse(body);
            var text = document.RootElement.TryGetProperty("response", out var responseText)
                ? responseText.GetString() ?? string.Empty
                : body;
            var finishReason = document.RootElement.TryGetProperty("done_reason", out var doneReason)
                ? doneReason.GetString() ?? string.Empty
                : "completed";

            return new AIResponse
            {
                Succeeded = true,
                Provider = Name,
                Model = request.ModelName,
                Text = text,
                Duration = stopwatch.Elapsed,
                PromptSize = request.Prompt.Length,
                FinishReason = finishReason,
                RawProviderResponse = body
            };
        }
        catch (TaskCanceledException ex)
        {
            stopwatch.Stop();
            return Failure(request, stopwatch.Elapsed, AIErrorKind.Timeout, $"Ollama request timed out: {ex.Message}");
        }
        catch (HttpRequestException ex)
        {
            stopwatch.Stop();
            return Failure(request, stopwatch.Elapsed, AIErrorKind.ProviderUnavailable, $"Ollama is unavailable: {ex.Message}");
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            return Failure(request, stopwatch.Elapsed, AIErrorKind.RuntimeCrash, $"Ollama provider crashed: {ex.Message}");
        }
    }

    private static CancellationTokenSource CreateTimeoutSource(AIRequest request, CancellationToken cancellationToken)
    {
        var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        if (request.Options.Timeout is { } timeout && timeout > TimeSpan.Zero)
        {
            timeoutSource.CancelAfter(timeout);
        }

        return timeoutSource;
    }

    private static object BuildOptions(AIExecutionOptions options)
    {
        var values = new Dictionary<string, object>();
        if (options.NumContext is { } numContext)
        {
            values["num_ctx"] = numContext;
        }

        if (options.NumPredict is { } numPredict)
        {
            values["num_predict"] = numPredict;
        }

        if (options.Temperature is { } temperature)
        {
            values["temperature"] = temperature;
        }

        if (options.TopP is { } topP)
        {
            values["top_p"] = topP;
        }

        if (options.Seed is { } seed)
        {
            values["seed"] = seed;
        }

        if (options.StopTokens.Count > 0)
        {
            values["stop"] = options.StopTokens;
        }

        foreach (var option in options.ProviderOptions)
        {
            values[option.Key] = option.Value;
        }

        return values;
    }

    private AIResponse Failure(
        AIRequest request,
        TimeSpan duration,
        AIErrorKind errorKind,
        string message,
        string rawResponse = "")
    {
        return new AIResponse
        {
            Succeeded = false,
            Provider = Name,
            Model = request.ModelName,
            Duration = duration,
            PromptSize = request.Prompt.Length,
            ErrorKind = errorKind,
            ErrorMessage = message,
            RawProviderResponse = rawResponse
        };
    }
}
