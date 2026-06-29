namespace Jarvis.Core.Agents.Coding.AI;

using Jarvis.Core.AI.Caching;
using Jarvis.Core.AI.Health;
using Jarvis.Core.AI.Prompt;
using Jarvis.Core.AI.Providers;
using Jarvis.Core.AI.Routing;
using Jarvis.Core.AI.Runtime;
using Jarvis.Services;

/// <summary>
/// Adapts the coding assistant model contract to the provider-independent AI runtime.
/// </summary>
public sealed class OllamaCodingModelClient : ICodingModelClient
{
    /// <summary>
    /// Defines the default coding model.
    /// </summary>
    public const string DefaultModel = "qwen2.5-coder:14b";

    private readonly AIRuntime runtime;
    private readonly int contextLength;

    /// <summary>
    /// Initializes a new instance of the <see cref="OllamaCodingModelClient"/> class.
    /// </summary>
    public OllamaCodingModelClient()
        : this(new HttpClient(), "http://localhost:11434", DefaultModel, 8192)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="OllamaCodingModelClient"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client.</param>
    /// <param name="settingsService">The settings service.</param>
    public OllamaCodingModelClient(HttpClient httpClient, SettingsService settingsService)
        : this(
            httpClient,
            settingsService.Current.OllamaBaseUrl,
            string.IsNullOrWhiteSpace(settingsService.Current.PreferredCodingModel)
                ? DefaultModel
                : settingsService.Current.PreferredCodingModel,
            settingsService.Current.OllamaContextLength)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="OllamaCodingModelClient"/> class.
    /// </summary>
    public OllamaCodingModelClient(HttpClient httpClient, string baseUrl, string modelName, int contextLength)
        : this(CreateRuntime(httpClient, baseUrl, modelName), contextLength)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="OllamaCodingModelClient"/> class.
    /// </summary>
    public OllamaCodingModelClient(AIRuntime runtime, int contextLength = 8192)
    {
        this.runtime = runtime;
        this.contextLength = contextLength <= 0 ? 8192 : contextLength;
    }

    /// <inheritdoc />
    public async Task<CodingModelResponse> GenerateAsync(
        CodingModelRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var aiRequest = new AIRequest
        {
            Purpose = "Coding",
            ProviderName = request.ProviderName,
            ModelName = request.ModelName,
            Prompt = request.Prompt,
            Context = new AIContext
            {
                Purpose = "Coding"
            },
            Options = new AIExecutionOptions
            {
                NumContext = contextLength,
                Timeout = TimeSpan.FromSeconds(45)
            }
        };

        foreach (var filePath in request.ContextFilePaths)
        {
            aiRequest.Context.FilePaths.Add(filePath);
        }

        foreach (var symbol in request.ContextSymbols)
        {
            aiRequest.Context.Symbols.Add(symbol);
        }

        var response = await runtime.ExecuteAsync(aiRequest, cancellationToken);
        return ToCodingResponse(response);
    }

    private static AIRuntime CreateRuntime(HttpClient httpClient, string baseUrl, string modelName)
    {
        var providerResolver = new ProviderResolver(
        [
            new OllamaProvider(httpClient, baseUrl),
            new OpenAIProvider(),
            new LMStudioProvider()
        ]);
        var router = new ModelRouter(
            string.IsNullOrWhiteSpace(modelName) ? DefaultModel : modelName,
            "Ollama");
        var healthChecker = new AIHealthChecker(providerResolver);
        return new AIRuntime(
            router,
            providerResolver,
            healthChecker,
            new PromptValidator(),
            new PromptCompressor(),
            new AIResponseCache());
    }

    private static CodingModelResponse ToCodingResponse(AIResponse response)
    {
        var codingResponse = new CodingModelResponse
        {
            Succeeded = response.Succeeded,
            Provider = response.Provider,
            ModelName = response.Model,
            Text = response.Text,
            ErrorMessage = response.ErrorMessage,
            Duration = response.Duration,
            OriginalPromptSize = response.OriginalPromptSize,
            CompressedPromptSize = response.CompressedPromptSize,
            FinishReason = response.FinishReason,
            RawProviderResponse = response.RawProviderResponse,
            FromCache = response.FromCache,
            ProviderHealthStatus = response.ProviderHealthStatus,
            ModelHealthStatus = response.ModelHealthStatus
        };
        foreach (var warning in response.Warnings)
        {
            codingResponse.Warnings.Add(warning);
        }

        return codingResponse;
    }
}
