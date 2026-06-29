namespace Jarvis.Core.AI.Routing;

using Jarvis.Core.AI.Providers;

/// <summary>
/// Resolves providers by name.
/// </summary>
public sealed class ProviderResolver
{
    private readonly Dictionary<string, IAIProvider> providers = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Initializes a new instance of the <see cref="ProviderResolver"/> class.
    /// </summary>
    public ProviderResolver(IEnumerable<IAIProvider> providers)
    {
        foreach (var provider in providers)
        {
            this.providers[provider.Name] = provider;
        }
    }

    /// <summary>
    /// Resolves a provider by name.
    /// </summary>
    public IAIProvider? Resolve(string providerName)
    {
        return providers.TryGetValue(providerName, out var provider) ? provider : null;
    }
}
