using System.Text.Json;
using Jarvis.Models;
using Jarvis.Repositories;
using Jarvis.Users;

namespace Jarvis.Services;

public sealed class SettingsService
{
    private readonly string _settingsPath;
    private readonly ISettingsRepository? _repository;
    private readonly JarvisUserContext _userContext;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public SettingsService(string settingsPath, ISettingsRepository? repository = null, JarvisUserContext? userContext = null)
    {
        _settingsPath = settingsPath;
        _repository = repository;
        _userContext = userContext ?? new JarvisUserContext();
    }

    public AppSettings Current { get; private set; } = new();

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        if (_repository is not null)
        {
            Current = await _repository.GetAsync(_userContext.UserId, "User", cancellationToken) ?? new AppSettings();
            Normalize(Current);
            return;
        }

        if (!File.Exists(_settingsPath))
        {
            Normalize(Current);
            await SaveAsync(cancellationToken);
            return;
        }

        var json = await File.ReadAllTextAsync(_settingsPath, cancellationToken);
        Current = JsonSerializer.Deserialize<AppSettings>(json, _jsonOptions) ?? new AppSettings();
        Normalize(Current);
    }

    public Task SaveAsync(CancellationToken cancellationToken = default)
    {
        Normalize(Current);

        if (_repository is not null)
        {
            return _repository.UpsertAsync(_userContext.UserId, "User", Current, cancellationToken);
        }

        var json = JsonSerializer.Serialize(Current, _jsonOptions);
        return File.WriteAllTextAsync(_settingsPath, json, cancellationToken);
    }

    public static void Normalize(AppSettings settings)
    {
        settings.OllamaContextLength = Math.Clamp(
            settings.OllamaContextLength <= 0 ? AppSettings.DefaultOllamaContextLength : settings.OllamaContextLength,
            512,
            32768);
    }
}
