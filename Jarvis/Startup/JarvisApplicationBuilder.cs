using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Jarvis.Startup;

public sealed class JarvisApplicationBuilder
{
    private readonly string[] _args;
    private readonly WebApplicationBuilder _builder;

    public JarvisApplicationBuilder(string[] args, WebApplicationBuilder builder, JarvisRuntime runtime)
    {
        _args = args;
        _builder = builder;
        Runtime = runtime;
    }

    public JarvisRuntime Runtime { get; }

    public WebApplication Build() => _builder.Build();

    public Task<bool> TryRunCliAsync()
    {
        if (!_args.Contains("--cli", StringComparer.OrdinalIgnoreCase))
        {
            return Task.FromResult(false);
        }

        return RunCliAndReturnAsync();
    }

    private async Task<bool> RunCliAndReturnAsync()
    {
        await CliBootstrapper.RunAsync(Runtime.SettingsService, Runtime.OllamaService, Runtime.Router);
        return true;
    }
}
