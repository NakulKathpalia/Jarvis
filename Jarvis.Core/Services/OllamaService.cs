using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Jarvis.Models;

namespace Jarvis.Services;

public sealed class OllamaService
{
    private readonly HttpClient _httpClient;
    private readonly SettingsService _settingsService;

    public OllamaService(HttpClient httpClient, SettingsService settingsService)
    {
        _httpClient = httpClient;
        _settingsService = settingsService;
    }

    public async Task<bool> IsRunningAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await _httpClient.GetAsync($"{_settingsService.Current.OllamaBaseUrl.TrimEnd('/')}/api/tags", cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async IAsyncEnumerable<string> StreamChatAsync(
        IEnumerable<ChatMessage> messages,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var request = new
        {
            model = _settingsService.Current.Model,
            messages = messages.Select(message => new { role = message.Role, content = message.Content }),
            stream = true
        };

        using var response = await _httpClient.PostAsJsonAsync(
            $"{_settingsService.Current.OllamaBaseUrl.TrimEnd('/')}/api/chat",
            request,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);

        while (!cancellationToken.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync(cancellationToken);
            if (line is null)
            {
                break;
            }

            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            using var document = JsonDocument.Parse(line);
            if (document.RootElement.TryGetProperty("message", out var message)
                && message.TryGetProperty("content", out var content))
            {
                yield return content.GetString() ?? string.Empty;
            }
        }
    }
}
