namespace Jarvis.Core.AI.Health;

using Jarvis.Core.AI.Routing;

/// <summary>
/// Performs provider and model health checks.
/// </summary>
public sealed class AIHealthChecker
{
    private readonly ProviderResolver providerResolver;

    /// <summary>
    /// Initializes a new instance of the <see cref="AIHealthChecker"/> class.
    /// </summary>
    public AIHealthChecker(ProviderResolver providerResolver)
    {
        this.providerResolver = providerResolver;
    }

    /// <summary>
    /// Checks provider health.
    /// </summary>
    public async Task<ProviderHealth> CheckProviderAsync(string providerName, CancellationToken cancellationToken = default)
    {
        var provider = providerResolver.Resolve(providerName);
        if (provider is null)
        {
            return new ProviderHealth
            {
                Provider = providerName,
                Available = false,
                Status = "UnknownProvider",
                FailureReason = $"Provider '{providerName}' is not registered."
            };
        }

        return await provider.GetHealthAsync(cancellationToken);
    }

    /// <summary>
    /// Checks model health.
    /// </summary>
    public async Task<ModelHealth> CheckModelAsync(ModelProfile profile, CancellationToken cancellationToken = default)
    {
        var providerHealth = await CheckProviderAsync(profile.ProviderName, cancellationToken);
        if (!providerHealth.Available)
        {
            return new ModelHealth
            {
                Provider = profile.ProviderName,
                Model = profile.ModelName,
                Available = false,
                Status = "ProviderUnavailable",
                FailureReason = providerHealth.FailureReason
            };
        }

        if (providerHealth.AvailableModels.Count > 0 &&
            !providerHealth.AvailableModels.Contains(profile.ModelName, StringComparer.OrdinalIgnoreCase))
        {
            return new ModelHealth
            {
                Provider = profile.ProviderName,
                Model = profile.ModelName,
                Available = false,
                Status = "ModelMissing",
                FailureReason = BuildMissingModelMessage(profile.ModelName, providerHealth.AvailableModels)
            };
        }

        return new ModelHealth
        {
            Provider = profile.ProviderName,
            Model = profile.ModelName,
            Available = true,
            Status = "Available"
        };
    }

    private static string BuildMissingModelMessage(string requestedModel, IEnumerable<string> availableModels)
    {
        var suggestion = availableModels.FirstOrDefault(model =>
            model.StartsWith("qwen2.5-coder", StringComparison.OrdinalIgnoreCase)) ?? "qwen2.5-coder:14b";
        return $"Requested model '{requestedModel}' is not installed for the selected provider. " +
            $"Installed model suggestion: '{suggestion}'. " +
            "To install the default coding model, run: ollama pull qwen2.5-coder:14b";
    }
}
