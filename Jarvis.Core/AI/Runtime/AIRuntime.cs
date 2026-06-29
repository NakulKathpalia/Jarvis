namespace Jarvis.Core.AI.Runtime;

using System.Diagnostics;
using Jarvis.Core.AI.Caching;
using Jarvis.Core.AI.Health;
using Jarvis.Core.AI.Prompt;
using Jarvis.Core.AI.Routing;

/// <summary>
/// Coordinates provider-independent AI execution.
/// </summary>
public sealed class AIRuntime
{
    private readonly ModelRouter modelRouter;
    private readonly ProviderResolver providerResolver;
    private readonly AIHealthChecker healthChecker;
    private readonly PromptValidator promptValidator;
    private readonly PromptCompressor promptCompressor;
    private readonly AIResponseCache responseCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="AIRuntime"/> class.
    /// </summary>
    public AIRuntime(
        ModelRouter modelRouter,
        ProviderResolver providerResolver,
        AIHealthChecker healthChecker,
        PromptValidator? promptValidator = null,
        PromptCompressor? promptCompressor = null,
        AIResponseCache? responseCache = null)
    {
        this.modelRouter = modelRouter;
        this.providerResolver = providerResolver;
        this.healthChecker = healthChecker;
        this.promptValidator = promptValidator ?? new PromptValidator();
        this.promptCompressor = promptCompressor ?? new PromptCompressor();
        this.responseCache = responseCache ?? new AIResponseCache();
    }

    /// <summary>
    /// Executes a request through the selected provider.
    /// </summary>
    public async Task<AIResponse> ExecuteAsync(AIRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var stopwatch = Stopwatch.StartNew();
        var profile = modelRouter.Route(request);
        var originalPrompt = request.Prompt ?? string.Empty;
        var validationWarnings = promptValidator.Validate(request);
        if (string.IsNullOrWhiteSpace(originalPrompt))
        {
            return Failure(profile, stopwatch.Elapsed, AIErrorKind.InvalidRequest, "AI request prompt is empty.", validationWarnings);
        }

        var compressedPrompt = promptCompressor.Compress(request);
        var executableRequest = CopyForExecution(request, profile, compressedPrompt);
        var cached = responseCache.Get(executableRequest, profile.ModelName);
        if (cached is not null)
        {
            cached.OriginalPromptSize = originalPrompt.Length;
            cached.CompressedPromptSize = compressedPrompt.Length;
            AddWarnings(cached, validationWarnings);
            return cached;
        }

        var modelHealth = await healthChecker.CheckModelAsync(profile, cancellationToken);
        if (!modelHealth.Available)
        {
            var errorKind = modelHealth.Status.Equals("ProviderUnavailable", StringComparison.OrdinalIgnoreCase)
                ? AIErrorKind.ProviderUnavailable
                : AIErrorKind.ModelMissing;
            var failure = Failure(profile, stopwatch.Elapsed, errorKind, modelHealth.FailureReason, validationWarnings);
            failure.ProviderHealthStatus = errorKind == AIErrorKind.ProviderUnavailable ? modelHealth.Status : "Available";
            failure.ModelHealthStatus = modelHealth.Status;
            return failure;
        }

        var provider = providerResolver.Resolve(profile.ProviderName);
        if (provider is null)
        {
            return Failure(
                profile,
                stopwatch.Elapsed,
                AIErrorKind.ProviderUnavailable,
                $"Provider '{profile.ProviderName}' is not registered.",
                validationWarnings);
        }

        var response = await provider.GenerateAsync(executableRequest, cancellationToken);
        response.Provider = profile.ProviderName;
        response.Model = profile.ModelName;
        response.OriginalPromptSize = originalPrompt.Length;
        response.CompressedPromptSize = compressedPrompt.Length;
        response.PromptSize = compressedPrompt.Length;
        response.ProviderHealthStatus = "Available";
        response.ModelHealthStatus = modelHealth.Status;
        if (response.Duration == TimeSpan.Zero)
        {
            response.Duration = stopwatch.Elapsed;
        }

        AddWarnings(response, validationWarnings);
        responseCache.Set(executableRequest, profile.ModelName, response);
        return response;
    }

    private static AIRequest CopyForExecution(AIRequest request, ModelProfile profile, string prompt)
    {
        var copy = new AIRequest
        {
            Purpose = request.Purpose,
            ProviderName = profile.ProviderName,
            ModelName = profile.ModelName,
            Prompt = prompt,
            Context = request.Context,
            Options = request.Options
        };
        return copy;
    }

    private static AIResponse Failure(
        ModelProfile profile,
        TimeSpan duration,
        AIErrorKind errorKind,
        string message,
        IEnumerable<string> warnings)
    {
        var response = new AIResponse
        {
            Succeeded = false,
            Provider = profile.ProviderName,
            Model = profile.ModelName,
            Duration = duration,
            ErrorKind = errorKind,
            ErrorMessage = message
        };
        AddWarnings(response, warnings);
        return response;
    }

    private static void AddWarnings(AIResponse response, IEnumerable<string> warnings)
    {
        foreach (var warning in warnings)
        {
            if (!response.Warnings.Contains(warning, StringComparer.OrdinalIgnoreCase))
            {
                response.Warnings.Add(warning);
            }
        }
    }
}
