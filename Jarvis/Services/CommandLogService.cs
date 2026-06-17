using System.Text.Json;
using System.Text.Json.Serialization;
using Jarvis.Models;

namespace Jarvis.Services;

public sealed class CommandLogService
{
    private readonly string _logPath;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };
    private readonly List<PcCommandLogEntry> _logs = [];
    private readonly SemaphoreSlim _gate = new(1, 1);

    public CommandLogService(string logPath)
    {
        _logPath = logPath;
        _jsonOptions.Converters.Add(new JsonStringEnumConverter());
    }

    public IReadOnlyCollection<PcCommandLogEntry> Logs => _logs;

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_logPath) ?? ".");
        if (!File.Exists(_logPath))
        {
            await SaveAsync(cancellationToken);
            return;
        }

        var json = await File.ReadAllTextAsync(_logPath, cancellationToken);
        var logs = JsonSerializer.Deserialize<List<PcCommandLogEntry>>(json, _jsonOptions) ?? [];
        _logs.Clear();
        _logs.AddRange(logs.OrderByDescending(log => log.TimestampUtc).Take(200));
    }

    public async Task AddAsync(PcCommandLogEntry entry, CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            entry.Id = string.IsNullOrWhiteSpace(entry.Id) ? Guid.NewGuid().ToString() : entry.Id;
            entry.TimestampUtc = entry.TimestampUtc == default ? DateTime.UtcNow : entry.TimestampUtc;
            _logs.Insert(0, entry);

            if (_logs.Count > 200)
            {
                _logs.RemoveRange(200, _logs.Count - 200);
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
        var json = JsonSerializer.Serialize(_logs, _jsonOptions);
        return File.WriteAllTextAsync(_logPath, json, cancellationToken);
    }
}
