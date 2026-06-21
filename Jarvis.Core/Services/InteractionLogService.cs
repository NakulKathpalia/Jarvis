using System.Text.Json;
using System.Text.Json.Serialization;
using Jarvis.Models;
using Jarvis.Repositories;
using Jarvis.Users;

namespace Jarvis.Services;

public sealed class InteractionLogService
{
    private readonly string _logPath;
    private readonly IAuditLogRepository? _repository;
    private readonly JarvisUserContext _userContext;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };
    private readonly List<InteractionLogEntry> _logs = [];
    private readonly SemaphoreSlim _gate = new(1, 1);

    public InteractionLogService(string logPath, IAuditLogRepository? repository = null, JarvisUserContext? userContext = null)
    {
        _logPath = logPath;
        _repository = repository;
        _userContext = userContext ?? new JarvisUserContext();
        _jsonOptions.Converters.Add(new JsonStringEnumConverter());
    }

    public IReadOnlyCollection<InteractionLogEntry> Logs => _logs;

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        if (_repository is not null)
        {
            var storedLogs = await _repository.GetRecentAsync(_userContext.UserId, 500, cancellationToken);
            _logs.Clear();
            _logs.AddRange(storedLogs.OrderByDescending(log => log.TimestampUtc).Take(500));
            return;
        }

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
            entry.UserId = _userContext.UserId;
            entry.DeviceId = _userContext.DeviceId;
            entry.TimestampUtc = entry.TimestampUtc == default ? DateTime.UtcNow : entry.TimestampUtc;
            entry.CreatedAtUtc = entry.CreatedAtUtc == default ? entry.TimestampUtc : entry.CreatedAtUtc;
            entry.UpdatedAtUtc = DateTime.UtcNow;
            _logs.Insert(0, entry);

            if (_logs.Count > 500)
            {
                _logs.RemoveRange(500, _logs.Count - 500);
            }

            if (_repository is not null)
            {
                await _repository.AddAsync(_userContext.UserId, entry, cancellationToken);
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

    public async Task ClearAsync(CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            _logs.Clear();
            if (_repository is not null)
            {
                await _repository.ClearAsync(_userContext.UserId, cancellationToken);
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
        var json = JsonSerializer.Serialize(_logs, _jsonOptions);
        return File.WriteAllTextAsync(_logPath, json, cancellationToken);
    }
}
