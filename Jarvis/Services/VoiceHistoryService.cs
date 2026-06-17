using System.Text.Json;
using System.Text.Json.Serialization;
using Jarvis.Models;

namespace Jarvis.Services;

public sealed class VoiceHistoryService
{
    private readonly string _historyPath;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };
    private readonly List<VoiceHistoryItem> _items = [];
    private readonly SemaphoreSlim _gate = new(1, 1);

    public VoiceHistoryService(string historyPath)
    {
        _historyPath = historyPath;
        _jsonOptions.Converters.Add(new JsonStringEnumConverter());
    }

    public IReadOnlyCollection<VoiceHistoryItem> Items => _items;

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
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
            _items.Insert(0, new VoiceHistoryItem
            {
                Transcript = result.Transcript,
                Response = string.IsNullOrWhiteSpace(result.AiResponse) ? result.Message : result.AiResponse,
                State = result.State,
                Success = result.Success,
                TimestampUtc = DateTime.UtcNow
            });

            if (_items.Count > 200)
            {
                _items.RemoveRange(200, _items.Count - 200);
            }

            await SaveAsync(cancellationToken);
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
