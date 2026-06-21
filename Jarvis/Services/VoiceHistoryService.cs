using System.Text.Json;
using System.Text.Json.Serialization;
using Jarvis.Models;
using Jarvis.Repositories;
using Jarvis.Users;

namespace Jarvis.Services;

public sealed class VoiceHistoryService
{
    private readonly string _historyPath;
    private readonly IVoiceHistoryRepository? _repository;
    private readonly JarvisUserContext _userContext;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };
    private readonly List<VoiceHistoryItem> _items = [];
    private readonly SemaphoreSlim _gate = new(1, 1);

    public VoiceHistoryService(string historyPath, IVoiceHistoryRepository? repository = null, JarvisUserContext? userContext = null)
    {
        _historyPath = historyPath;
        _repository = repository;
        _userContext = userContext ?? new JarvisUserContext();
        _jsonOptions.Converters.Add(new JsonStringEnumConverter());
    }

    public IReadOnlyCollection<VoiceHistoryItem> Items => _items;

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        if (_repository is not null)
        {
            var storedItems = await _repository.GetRecentAsync(_userContext.UserId, 200, cancellationToken);
            _items.Clear();
            _items.AddRange(storedItems.OrderByDescending(item => item.TimestampUtc).Take(200));
            return;
        }

        Directory.CreateDirectory(Path.GetDirectoryName(_historyPath) ?? ".");
        if (!File.Exists(_historyPath))
        {
            await SaveAsync(cancellationToken);
            return;
        }

        var json = await File.ReadAllTextAsync(_historyPath, cancellationToken);
        var items = JsonSerializer.Deserialize<List<VoiceHistoryItem>>(json, _jsonOptions) ?? [];
        _items.Clear();
        _items.AddRange(items.OrderByDescending(item => item.TimestampUtc).Take(200));
    }

    public async Task AddAsync(VoicePipelineResult result, CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            var item = new VoiceHistoryItem
            {
                UserId = _userContext.UserId,
                DeviceId = _userContext.DeviceId,
                Transcript = result.Transcript,
                Response = string.IsNullOrWhiteSpace(result.AiResponse) ? result.Message : result.AiResponse,
                State = result.State,
                Success = result.Success,
                TimestampUtc = DateTime.UtcNow
            };
            _items.Insert(0, item);

            if (_items.Count > 200)
            {
                _items.RemoveRange(200, _items.Count - 200);
            }

            if (_repository is not null)
            {
                await _repository.AddAsync(_userContext.UserId, item, cancellationToken);
            }
            else
            {
                await SaveAsync(cancellationToken);
            }
        }
        finally
        {
            _gate.Release();
        }
    }

    private Task SaveAsync(CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(_items, _jsonOptions);
        return File.WriteAllTextAsync(_historyPath, json, cancellationToken);
    }
}
