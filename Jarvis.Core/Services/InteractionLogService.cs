using System.Text.Json;
using System.Text.Json.Serialization;
using Jarvis.Models;

namespace Jarvis.Services;

public sealed class InteractionLogService
{
    private readonly string _logPath;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };
    private readonly List<InteractionLogEntry> _logs = [];
    private readonly SemaphoreSlim _gate = new(1, 1);

    public InteractionLogService(string logPath)
    {
        _logPath = logPath;
        _jsonOptions.Converters.Add(new JsonStringEnumConverter());
    }

    public IReadOnlyCollection<InteractionLogEntry> Logs => _logs;

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_logPath) ?? ".");
        if (!File.Exists(_logPath))
        {
            await SaveAsync(cancellationToken);
            return;
        }

        var json = await File.ReadAllTextAsync(_logPath, cancellationToken);
        var logs = JsonSerializer.Deserialize<List<InteractionLogEntry>>(json, _jsonOptions) ?? [];
        _logs.Clear();
        _logs.AddRange(logs.OrderByDescending(log => log.TimestampUtc).Take(500));
    }

    public Task AddAsync(
        InteractionSource source,
        InteractionType type,
        string stage,
        InteractionStatus status,
        string message,
        string input = "",
        string output = "",
        string error = "",
        Dictionary<string, JsonElement>? metadata = null,
        CancellationToken cancellationToken = default)
    {
        return AddAsync(new InteractionLogEntry
        {
            Source = source,
            Type = type,
            Stage = stage,
            Input = input,
            Output = output,
            Status = status,
            Message = message,
            Error = error,
            Metadata = metadata ?? []
        }, cancellationToken);
    }

    public async Task AddAsync(InteractionLogEntry entry, CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            entry.Id = string.IsNullOrWhiteSpace(entry.Id) ? Guid.NewGuid().ToString("N") : entry.Id;
            entry.TimestampUtc = entry.TimestampUtc == default ? DateTime.UtcNow : entry.TimestampUtc;
            _logs.Insert(0, entry);

            if (_logs.Count > 500)
            {
                _logs.RemoveRange(500, _logs.Count - 500);
            }

            await SaveAsync(cancellationToken);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task ClearAsync(CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            _logs.Clear();
            await SaveAsync(cancellationToken);
        }
        finally
        {
            _gate.Release();
        }
    }

    private Task SaveAsync(CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(_logs, _jsonOptions);
        return File.WriteAllTextAsync(_logPath, json, cancellationToken);
    }
}
