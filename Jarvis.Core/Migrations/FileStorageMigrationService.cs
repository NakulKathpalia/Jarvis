using System.Text.Json;
using System.Text.Json.Serialization;
using Jarvis.Models;
using Jarvis.Repositories;
using Jarvis.Users;

namespace Jarvis.Migrations;

public sealed class FileStorageMigrationService
{
    private const string MigrationName = "json-to-local-mongodb";
    private const string MigrationVersion = "0.6";

    private readonly FileStorageMigrationPaths _paths;
    private readonly JarvisUserContext _userContext;
    private readonly IMigrationRepository _migrationRepository;
    private readonly IMemoryRepository _memoryRepository;
    private readonly IChatRepository _chatRepository;
    private readonly IChatHistoryRepository _chatHistoryRepository;
    private readonly ISettingsRepository _settingsRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICommandHistoryRepository _commandHistoryRepository;
    private readonly IVoiceHistoryRepository _voiceHistoryRepository;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public FileStorageMigrationService(
        FileStorageMigrationPaths paths,
        JarvisUserContext userContext,
        IMigrationRepository migrationRepository,
        IMemoryRepository memoryRepository,
        IChatRepository chatRepository,
        IChatHistoryRepository chatHistoryRepository,
        ISettingsRepository settingsRepository,
        IAuditLogRepository auditLogRepository,
        ICommandHistoryRepository commandHistoryRepository,
        IVoiceHistoryRepository voiceHistoryRepository)
    {
        _paths = paths;
        _userContext = userContext;
        _migrationRepository = migrationRepository;
        _memoryRepository = memoryRepository;
        _chatRepository = chatRepository;
        _chatHistoryRepository = chatHistoryRepository;
        _settingsRepository = settingsRepository;
        _auditLogRepository = auditLogRepository;
        _commandHistoryRepository = commandHistoryRepository;
        _voiceHistoryRepository = voiceHistoryRepository;
        _jsonOptions.Converters.Add(new JsonStringEnumConverter());
    }

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        if (await _migrationRepository.HasCompletedAsync(MigrationName, MigrationVersion, cancellationToken))
        {
            return;
        }

        var imported = 0;
        imported += await ImportMemoriesAsync(cancellationToken);
        imported += await ImportChatHistoryAsync(cancellationToken);
        imported += await ImportChatSessionsAsync(cancellationToken);
        imported += await ImportSettingsAsync(cancellationToken);
        imported += await ImportAuditLogsAsync(cancellationToken);
        imported += await ImportCommandHistoryAsync(cancellationToken);
        imported += await ImportVoiceHistoryAsync(cancellationToken);

        await _migrationRepository.CompleteAsync(
            MigrationName,
            MigrationVersion,
            $"Imported {imported} existing JSON records. JSON files were left in place for rollback.",
            cancellationToken);
    }

    private async Task<int> ImportMemoriesAsync(CancellationToken cancellationToken)
    {
        var items = await ReadJsonAsync<List<MemoryItem>>(_paths.MemoryPath, cancellationToken) ?? [];
        foreach (var item in items)
        {
            item.UserId = _userContext.UserId;
            await _memoryRepository.UpsertAsync(_userContext.UserId, item, cancellationToken);
        }

        return items.Count;
    }

    private async Task<int> ImportChatSessionsAsync(CancellationToken cancellationToken)
    {
        var sessions = await ReadJsonAsync<List<ChatSession>>(_paths.ChatSessionsPath, cancellationToken) ?? [];
        var count = 0;
        foreach (var session in sessions)
        {
            session.UserId = _userContext.UserId;
            var messages = session.Messages.ToList();
            session.Messages = [];
            await _chatRepository.UpsertSessionAsync(_userContext.UserId, session, cancellationToken);
            foreach (var message in messages)
            {
                message.UserId = _userContext.UserId;
                message.ChatSessionId = session.Id;
                await _chatRepository.AddMessageAsync(_userContext.UserId, session.Id, message, cancellationToken);
                count++;
            }

            count++;
        }

        return count;
    }

    private async Task<int> ImportChatHistoryAsync(CancellationToken cancellationToken)
    {
        var messages = await ReadJsonAsync<List<ChatMessage>>(_paths.ChatHistoryPath, cancellationToken) ?? [];
        foreach (var message in messages)
        {
            message.UserId = _userContext.UserId;
            await _chatHistoryRepository.AddAsync(_userContext.UserId, message, cancellationToken);
        }

        return messages.Count;
    }

    private async Task<int> ImportSettingsAsync(CancellationToken cancellationToken)
    {
        var settings = await ReadJsonAsync<AppSettings>(_paths.SettingsPath, cancellationToken);
        if (settings is null)
        {
            return 0;
        }

        await _settingsRepository.UpsertAsync(_userContext.UserId, "User", settings, cancellationToken);
        return 1;
    }

    private async Task<int> ImportAuditLogsAsync(CancellationToken cancellationToken)
    {
        var logs = await ReadJsonAsync<List<InteractionLogEntry>>(_paths.InteractionLogPath, cancellationToken) ?? [];
        foreach (var log in logs)
        {
            log.UserId = _userContext.UserId;
            log.DeviceId = _userContext.DeviceId;
            await _auditLogRepository.AddAsync(_userContext.UserId, log, cancellationToken);
        }

        return logs.Count;
    }

    private async Task<int> ImportCommandHistoryAsync(CancellationToken cancellationToken)
    {
        var logs = await ReadJsonAsync<List<PcCommandLogEntry>>(_paths.CommandLogPath, cancellationToken) ?? [];
        foreach (var log in logs)
        {
            log.UserId = _userContext.UserId;
            log.DeviceId = _userContext.DeviceId;
            await _commandHistoryRepository.AddAsync(_userContext.UserId, log, cancellationToken);
        }

        return logs.Count;
    }

    private async Task<int> ImportVoiceHistoryAsync(CancellationToken cancellationToken)
    {
        var items = await ReadJsonAsync<List<VoiceHistoryItem>>(_paths.VoiceHistoryPath, cancellationToken) ?? [];
        foreach (var item in items)
        {
            item.UserId = _userContext.UserId;
            item.DeviceId = _userContext.DeviceId;
            await _voiceHistoryRepository.AddAsync(_userContext.UserId, item, cancellationToken);
        }

        return items.Count;
    }

    private async Task<T?> ReadJsonAsync<T>(string path, CancellationToken cancellationToken)
    {
        if (!File.Exists(path))
        {
            return default;
        }

        var json = await File.ReadAllTextAsync(path, cancellationToken);
        if (string.IsNullOrWhiteSpace(json))
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(json, _jsonOptions);
    }
}
