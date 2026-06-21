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
            return;
        }

        if (!File.Exists(_settingsPath))
        {
            await SaveAsync(cancellationToken);
            return;
        }

        var json = await File.ReadAllTextAsync(_settingsPath, cancellationToken);
        Current = JsonSerializer.Deserialize<AppSettings>(json, _jsonOptions) ?? new AppSettings();
    }

    public Task SaveAsync(CancellationToken cancellationToken = default)
    {
        if (_repository is not null)
        {
            return _repository.UpsertAsync(_userContext.UserId, "User", Current, cancellationToken);
        }

        var json = JsonSerializer.Serialize(Current, _jsonOptions);
        return File.WriteAllTextAsync(_settingsPath, json, cancellationToken);
    }
}
