namespace Jarvis.Core.AI.Routing;

using Jarvis.Core.AI.Runtime;

/// <summary>
/// Selects a provider and model for an AI request.
/// </summary>
public sealed class ModelRouter
{
    private readonly string preferredCodingModel;
    private readonly string defaultProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="ModelRouter"/> class.
    /// </summary>
    public ModelRouter(string preferredCodingModel = "qwen2.5-coder:14b", string defaultProvider = "Ollama")
    {
        this.preferredCodingModel = string.IsNullOrWhiteSpace(preferredCodingModel)
            ? "qwen2.5-coder:14b"
            : preferredCodingModel;
        this.defaultProvider = string.IsNullOrWhiteSpace(defaultProvider) ? "Ollama" : defaultProvider;
    }

    /// <summary>
    /// Routes a request to a model profile.
    /// </summary>
    public ModelProfile Route(AIRequest request)
    {
        var purpose = string.IsNullOrWhiteSpace(request.Purpose) ? "General" : request.Purpose;
        var model = string.IsNullOrWhiteSpace(request.ModelName) && purpose.Equals("Coding", StringComparison.OrdinalIgnoreCase)
            ? preferredCodingModel
            : request.ModelName;

        if (string.IsNullOrWhiteSpace(model))
        {
            model = preferredCodingModel;
        }

        return new ModelProfile
        {
            Purpose = purpose,
            ProviderName = string.IsNullOrWhiteSpace(request.ProviderName) ? defaultProvider : request.ProviderName,
            ModelName = model
        };
    }
}
