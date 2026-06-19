using Jarvis.Commands;
using Jarvis.Core;
using Jarvis.Data;
using Jarvis.Memory;
using Jarvis.Models;
using Jarvis.Services;
using System.Text.Json.Serialization;

var basePath = AppContext.BaseDirectory;
var pathResolver = new PathResolver(basePath);
var platformService = new PlatformService();

var settingsService = new SettingsService(pathResolver.SettingsPath);
await settingsService.LoadAsync();

var memoryService = new MemoryService(pathResolver.MemoryPath);
await memoryService.LoadAsync();

var chatHistoryService = new ChatHistoryService(pathResolver.ChatHistoryPath);
await chatHistoryService.LoadAsync();

var chatSessionService = new ChatSessionService(pathResolver.ChatSessionsPath);
await chatSessionService.LoadAsync();

var commandLogService = new CommandLogService(pathResolver.CommandLogPath);
await commandLogService.LoadAsync();

var voiceHistoryService = new VoiceHistoryService(pathResolver.VoiceHistoryPath);
await voiceHistoryService.LoadAsync();

using var httpClient = new HttpClient();
var ollamaService = new OllamaService(httpClient, settingsService);
var whisperService = new WhisperService(settingsService);
var piperService = new PiperService(settingsService, pathResolver.GeneratedAudioDirectory);
var wakeWordService = new WakeWordService(settingsService, whisperService);
var fileIndexService = new FileIndexService(settingsService);
var pcCommandParser = new PcCommandParser();
var commandSafetyService = new CommandSafetyService();
var settingsValidationService = new SettingsValidationService(settingsService);
var pcControlService = new WindowsPcControlService(pathResolver.ScreenshotDirectory);
var pcCommandService = new PcCommandService(pcCommandParser, commandSafetyService, commandLogService, pcControlService);
var voiceCommandService = new VoiceCommandService(memoryService, fileIndexService, settingsService, pcCommandService);
var classifierService = new ClassifierService();

var commandManager = Commands.Create(
    memoryService,
    settingsService,
    fileIndexService,
    pcCommandService);

var assistant = new Assistant(
    ollamaService,
    settingsService,
    memoryService,
    chatHistoryService);

var voicePipelineService = new VoicePipelineService(
    whisperService,
    wakeWordService,
    pcCommandParser,
    pcCommandService,
    assistant,
    piperService,
    settingsService,
    voiceHistoryService);

var router = new CommandRouter(classifierService, commandManager, assistant);

if (args.Contains("--cli", StringComparer.OrdinalIgnoreCase))
{
    await RunCliAsync(settingsService, ollamaService, router);
    return;
}

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://localhost:5055");
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

var app = builder.Build();
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/api/status", async () => Results.Ok(new
{
    online = await ollamaService.IsRunningAsync(),
    settingsService.Current.Model,
    settingsService.Current.OllamaBaseUrl,
    memoryCount = memoryService.Items.Count,
    historyCount = chatHistoryService.Messages.Count
}));

app.MapGet("/api/diagnostics", async () =>
{
    var validation = settingsValidationService.Validate();
    var ollamaOnline = await ollamaService.IsRunningAsync();
    return Results.Ok(new DiagnosticsResult(
        platformService.Current,
        pathResolver.AppDataDirectory,
        pathResolver.MemoryPath,
        pathResolver.LogsDirectory,
        pathResolver.ScreenshotDirectory,
        pathResolver.GeneratedAudioDirectory,
        new ServiceDiagnostic(ollamaOnline, ollamaOnline
            ? "Ollama is reachable."
            : "Ollama is not reachable."),
        new ServiceDiagnostic(whisperService.IsConfigured, whisperService.StatusMessage),
        new ServiceDiagnostic(piperService.IsConfigured, piperService.StatusMessage),
        validation.Warnings));
});

app.MapGet("/api/history", () => Results.Ok(chatHistoryService.Messages));

app.MapPost("/api/chat", async (ChatRequest request, CancellationToken cancellationToken) =>
{
    if (string.IsNullOrWhiteSpace(request.Message))
    {
        return Results.BadRequest(new { error = "Message is required." });
    }

    var response = await assistant.GenerateResponseAsync(request.Message.Trim(), cancellationToken: cancellationToken);
    return Results.Ok(new { response });
});

app.MapPost("/api/assistant/input", async (AssistantInputRequest request, CancellationToken cancellationToken) =>
{
    if (string.IsNullOrWhiteSpace(request.Message))
    {
        return Results.BadRequest(new { error = "Message is required." });
    }

    var session = string.IsNullOrWhiteSpace(request.ChatSessionId)
        ? await chatSessionService.CreateAsync(cancellationToken: cancellationToken)
        : chatSessionService.Get(request.ChatSessionId);

    if (session is null)
    {
        return Results.NotFound(new { error = "Chat session not found." });
    }

    var message = request.Message.Trim();
    await chatSessionService.AddMessageAsync(session.Id, ChatMessage.User(message), cancellationToken);

    var parsedCommand = pcCommandParser.Parse(message);
    if (parsedCommand.Action != PcControlAction.Unknown)
    {
        var commandResult = await pcCommandService.ExecuteAsync(message, cancellationToken: cancellationToken);
        var assistantMessage = commandResult.RequiresConfirmation
            ? BuildConfirmationMessage(commandResult)
            : commandResult.Message;

        await chatSessionService.AddMessageAsync(session.Id, ChatMessage.Assistant(assistantMessage), cancellationToken);

        return Results.Ok(new AssistantInputResponse(
            "command",
            commandResult.Handled,
            commandResult.RequiresConfirmation,
            commandResult.Command,
            commandResult.Target,
            commandResult.Message,
            null,
            commandResult.ConfirmationId ?? commandResult.ConfirmationToken,
            chatSessionService.Get(session.Id)));
    }

    if (TryBuildLocalAssistantResponse(message, out var localResponse))
    {
        await chatSessionService.AddMessageAsync(session.Id, ChatMessage.Assistant(localResponse), cancellationToken);
        return Results.Ok(new AssistantInputResponse(
            "chat",
            true,
            false,
            string.Empty,
            string.Empty,
            localResponse,
            localResponse,
            null,
            chatSessionService.Get(session.Id)));
    }

    var refreshedSession = chatSessionService.Get(session.Id);
    var response = await assistant.GenerateSessionResponseAsync(
        message,
        refreshedSession?.Messages ?? [ChatMessage.User(message)],
        cancellationToken: cancellationToken);

    if (!string.IsNullOrWhiteSpace(response))
    {
        await chatSessionService.AddMessageAsync(session.Id, ChatMessage.Assistant(response), cancellationToken);
    }

    return Results.Ok(new AssistantInputResponse(
        "chat",
        true,
        false,
        string.Empty,
        string.Empty,
        response,
        response,
        null,
        chatSessionService.Get(session.Id)));
});

app.MapPost("/api/assistant/confirm", async (AssistantConfirmRequest request, CancellationToken cancellationToken) =>
{
    if (string.IsNullOrWhiteSpace(request.ConfirmationId))
    {
        return Results.BadRequest(new { error = "Confirmation id is required." });
    }

    var result = await pcCommandService.ConfirmAsync(request.ConfirmationId, cancellationToken);

    ChatSession? session = null;
    if (!string.IsNullOrWhiteSpace(request.ChatSessionId))
    {
        session = chatSessionService.Get(request.ChatSessionId);
        if (session is not null)
        {
            await chatSessionService.AddMessageAsync(session.Id, ChatMessage.Assistant(result.Message), cancellationToken);
            session = chatSessionService.Get(session.Id);
        }
    }

    return Results.Ok(new AssistantInputResponse(
        "command",
        result.Handled,
        false,
        result.Command,
        result.Target,
        result.Message,
        null,
        null,
        session));
});

app.MapGet("/api/chats", () => Results.Ok(chatSessionService.GetSummaries()));

app.MapGet("/api/chats/{id}", (string id) =>
{
    var session = chatSessionService.Get(id);
    return session is null
        ? Results.NotFound(new { error = "Chat session not found." })
        : Results.Ok(session);
});

app.MapPost("/api/chats", async (ChatSessionCreateRequest request, CancellationToken cancellationToken) =>
{
    var session = await chatSessionService.CreateAsync(request.Title, cancellationToken);
    return Results.Ok(session);
});

app.MapPost("/api/chats/{id}/messages", async (string id, ChatSessionMessageRequest request, CancellationToken cancellationToken) =>
{
    if (string.IsNullOrWhiteSpace(request.Message))
    {
        return Results.BadRequest(new { error = "Message is required." });
    }

    var session = chatSessionService.Get(id);
    if (session is null)
    {
        return Results.NotFound(new { error = "Chat session not found." });
    }

    var userMessage = ChatMessage.User(request.Message.Trim());
    await chatSessionService.AddMessageAsync(id, userMessage, cancellationToken);

    var refreshedSession = chatSessionService.Get(id);
    var response = await assistant.GenerateSessionResponseAsync(
        request.Message.Trim(),
        refreshedSession?.Messages ?? [userMessage],
        cancellationToken: cancellationToken);

    if (!string.IsNullOrWhiteSpace(response))
    {
        await chatSessionService.AddMessageAsync(id, ChatMessage.Assistant(response), cancellationToken);
    }

    return Results.Ok(new
    {
        response,
        session = chatSessionService.Get(id)
    });
});

app.MapDelete("/api/chats/{id}", async (string id, CancellationToken cancellationToken) =>
{
    var deleted = await chatSessionService.DeleteAsync(id, cancellationToken);
    return deleted
        ? Results.Ok(chatSessionService.GetSummaries())
        : Results.NotFound(new { error = "Chat session not found." });
});

app.MapGet("/api/memory", () => Results.Ok(memoryService.Items));

app.MapPost("/api/memory", async (MemoryRequest request, CancellationToken cancellationToken) =>
{
    if (string.IsNullOrWhiteSpace(request.Text))
    {
        return Results.BadRequest(new { error = "Memory text is required." });
    }

    await memoryService.AddAsync(
        request.Text.Trim(),
        request.Category ?? "General",
        request.Tags,
        request.Importance,
        cancellationToken);
    return Results.Ok(memoryService.Items);
});

app.MapGet("/api/memory/search", (string? q, string? category, string? tag, int? minImportance) =>
{
    var results = memoryService.Search(q ?? string.Empty, category, tag, minImportance);
    return Results.Ok(results);
});

app.MapPut("/api/memory/{id}", async (string id, MemoryUpdateRequest request, CancellationToken cancellationToken) =>
{
    if (string.IsNullOrWhiteSpace(request.Text))
    {
        return Results.BadRequest(new { error = "Memory text is required." });
    }

    var updated = await memoryService.UpdateAsync(
        id,
        request.Text.Trim(),
        request.Category ?? "General",
        request.Tags,
        request.Importance ?? 3,
        cancellationToken);

    return updated is null
        ? Results.NotFound(new { error = "Memory not found." })
        : Results.Ok(memoryService.Items);
});

app.MapDelete("/api/memory/{id}", async (string id, CancellationToken cancellationToken) =>
{
    var removed = await memoryService.DeleteAsync(id, cancellationToken);
    return removed is null
        ? Results.NotFound(new { error = "Memory not found." })
        : Results.Ok(memoryService.Items);
});

app.MapDelete("/api/memory", async (CancellationToken cancellationToken) =>
{
    await memoryService.ClearAsync(cancellationToken);
    return Results.Ok(memoryService.Items);
});

app.MapGet("/api/settings", () => Results.Ok(settingsService.Current));

app.MapPost("/api/settings", async (AppSettings request, CancellationToken cancellationToken) =>
{
    settingsService.Current.OllamaBaseUrl = request.OllamaBaseUrl;
    settingsService.Current.Model = request.Model;
    settingsService.Current.SystemPrompt = request.SystemPrompt;
    settingsService.Current.MaxHistoryMessages = Math.Max(1, request.MaxHistoryMessages);
    settingsService.Current.FileIndexRoot = request.FileIndexRoot;
    settingsService.Current.WhisperExecutablePath = request.WhisperExecutablePath;
    settingsService.Current.WhisperModelPath = request.WhisperModelPath;
    settingsService.Current.WhisperLanguage = request.WhisperLanguage;
    settingsService.Current.PiperExecutablePath = request.PiperExecutablePath;
    settingsService.Current.PiperModelPath = request.PiperModelPath;
    settingsService.Current.AutoSpeakResponses = request.AutoSpeakResponses;
    settingsService.Current.WakeWordEnabled = request.WakeWordEnabled;
    settingsService.Current.WakeWordPhrase = request.WakeWordPhrase;
    settingsService.Current.WakeWordDetectorPath = request.WakeWordDetectorPath;
    settingsService.Current.WakeWordModelPath = request.WakeWordModelPath;

    await settingsService.SaveAsync(cancellationToken);
    return Results.Ok(settingsService.Current);
});

app.MapPost("/api/files/index", async (CancellationToken cancellationToken) =>
{
    var count = await fileIndexService.RebuildAsync(cancellationToken);
    return Results.Ok(new { count });
});

app.MapGet("/api/files/search", (string q) =>
{
    return Results.Ok(fileIndexService.Search(q));
});

app.MapGet("/api/files/search-detailed", (string q, int? limit) =>
{
    var results = fileIndexService.SearchDetailed(q, limit.GetValueOrDefault(25));
    return Results.Ok(results);
});

app.MapPost("/api/files/open", (FileOpenRequest request) =>
{
    if (string.IsNullOrWhiteSpace(request.Path))
    {
        return Results.BadRequest(new { error = "Path is required." });
    }

    var success = fileIndexService.OpenFile(request.Path);
    return success
        ? Results.Ok(new { success = true, message = "File opened." })
        : Results.BadRequest(new { error = "Unable to open file." });
});

app.MapPost("/api/files/open-folder", (FileOpenRequest request) =>
{
    if (string.IsNullOrWhiteSpace(request.Path))
    {
        return Results.BadRequest(new { error = "Path is required." });
    }

    var success = fileIndexService.OpenContainingFolder(request.Path);
    return success
        ? Results.Ok(new { success = true, message = "Folder opened." })
        : Results.BadRequest(new { error = "Unable to open folder." });
});

app.MapGet("/api/commands/catalog", () => Results.Ok(pcCommandService.Catalog));

app.MapGet("/api/commands/logs", () => Results.Ok(commandLogService.Logs.Take(50)));

app.MapPost("/api/commands/execute", async (PcCommandExecuteRequest request, CancellationToken cancellationToken) =>
{
    if (string.IsNullOrWhiteSpace(request.Input))
    {
        return Results.BadRequest(new { error = "Command input is required." });
    }

    var result = await pcCommandService.ExecuteAsync(request.Input.Trim(), cancellationToken: cancellationToken);
    return Results.Ok(result);
});

app.MapPost("/api/commands/confirm", async (PcCommandConfirmRequest request, CancellationToken cancellationToken) =>
{
    if (string.IsNullOrWhiteSpace(request.ConfirmationId))
    {
        return Results.BadRequest(new { error = "Confirmation id is required." });
    }

    var result = await pcCommandService.ConfirmAsync(request.ConfirmationId.Trim(), cancellationToken);
    return Results.Ok(result);
});

app.MapGet("/api/voice/status", () => Results.Ok(new
{
    whisper = new
    {
        configured = whisperService.IsConfigured,
        message = whisperService.StatusMessage,
        settingsService.Current.WhisperExecutablePath,
        settingsService.Current.WhisperModelPath,
        settingsService.Current.WhisperLanguage
    },
    piper = new
    {
        configured = piperService.IsConfigured,
        message = piperService.StatusMessage,
        settingsService.Current.PiperExecutablePath,
        settingsService.Current.PiperModelPath,
        settingsService.Current.AutoSpeakResponses
    },
    wakeWord = new
    {
        enabled = settingsService.Current.WakeWordEnabled,
        configured = wakeWordService.IsConfigured,
        mode = wakeWordService.Mode,
        message = wakeWordService.StatusMessage,
        settingsService.Current.WakeWordPhrase,
        settingsService.Current.WakeWordDetectorPath,
        settingsService.Current.WakeWordModelPath
    }
}));

app.MapGet("/api/voice/wake-status", () => Results.Ok(new
{
    enabled = settingsService.Current.WakeWordEnabled,
    configured = wakeWordService.IsConfigured,
    mode = wakeWordService.Mode,
    message = wakeWordService.StatusMessage,
    settingsService.Current.WakeWordPhrase,
    settingsService.Current.WakeWordDetectorPath,
    settingsService.Current.WakeWordModelPath
}));

app.MapPost("/api/voice/wake-check", (WakeWordCheckRequest request) =>
{
    if (string.IsNullOrWhiteSpace(request.Transcript))
    {
        return Results.BadRequest(new { error = "Transcript is required." });
    }

    return Results.Ok(wakeWordService.CheckTranscript(request.Transcript));
});

app.MapGet("/api/voice/commands", () => Results.Ok(voiceCommandService.GetCatalog()));

app.MapGet("/api/voice/tts-status", () => Results.Ok(new
{
    configured = piperService.IsConfigured,
    message = piperService.StatusMessage,
    settingsService.Current.PiperExecutablePath,
    settingsService.Current.PiperModelPath,
    settingsService.Current.AutoSpeakResponses
}));

app.MapPost("/api/voice/transcribe", async (HttpRequest request, CancellationToken cancellationToken) =>
{
    if (!request.HasFormContentType)
    {
        return Results.BadRequest(new { error = "Audio upload must be multipart/form-data." });
    }

    var form = await request.ReadFormAsync(cancellationToken);
    var audio = form.Files.GetFile("audio");
    if (audio is null || audio.Length == 0)
    {
        return Results.BadRequest(new { error = "Audio file is required." });
    }

    var result = await whisperService.TranscribeAsync(audio, cancellationToken);
    return Results.Ok(new
    {
        transcript = result.Transcript,
        ready = result.IsReady,
        succeeded = result.Succeeded,
        fileName = audio.FileName,
        contentType = audio.ContentType,
        sizeBytes = audio.Length,
        message = result.Message
    });
});

app.MapGet("/api/voice/pipeline/status", () => Results.Ok(voicePipelineService.Status));

app.MapGet("/api/voice/history", () => Results.Ok(voiceHistoryService.Items.Take(50)));

app.MapPost("/api/voice/pipeline", async (HttpRequest request, CancellationToken cancellationToken) =>
{
    if (!request.HasFormContentType)
    {
        return Results.BadRequest(new { error = "Audio upload must be multipart/form-data." });
    }

    var form = await request.ReadFormAsync(cancellationToken);
    var audio = form.Files.GetFile("audio");
    if (audio is null || audio.Length == 0)
    {
        return Results.BadRequest(new { error = "Audio file is required." });
    }

    var requireWakeWord = bool.TryParse(form["requireWakeWord"], out var parsedRequireWakeWord)
        && parsedRequireWakeWord;

    var result = await voicePipelineService.ProcessAsync(audio, requireWakeWord, cancellationToken);
    return Results.Ok(result);
});

app.MapPost("/api/voice/pipeline/confirm", async (VoiceConfirmationRequest request, CancellationToken cancellationToken) =>
{
    if (string.IsNullOrWhiteSpace(request.ConfirmationId))
    {
        return Results.BadRequest(new { error = "Confirmation id is required." });
    }

    var result = await voicePipelineService.ConfirmAsync(request.ConfirmationId.Trim(), cancellationToken);
    return Results.Ok(result);
});

app.MapPost("/api/voice/speak", async (SpeakRequest request, CancellationToken cancellationToken) =>
{
    if (string.IsNullOrWhiteSpace(request.Text))
    {
        return Results.BadRequest(new { error = "Text is required." });
    }

    var result = await piperService.SpeakAsync(request.Text.Trim(), cancellationToken);
    return Results.Ok(new
    {
        audioUrl = result.AudioUrl,
        ready = result.IsReady,
        succeeded = result.Succeeded,
        message = result.Message
    });
});

app.MapPost("/api/voice/command", async (VoiceCommandRequest request, CancellationToken cancellationToken) =>
{
    if (string.IsNullOrWhiteSpace(request.Transcript))
    {
        return Results.BadRequest(new { error = "Transcript is required." });
    }

    var result = await voiceCommandService.TryExecuteAsync(
        request.Transcript,
        request.Confirmed,
        cancellationToken);

    if (result.Handled)
    {
        await chatHistoryService.AddAsync(ChatMessage.User(request.Transcript.Trim()), cancellationToken);
        await chatHistoryService.AddAsync(ChatMessage.Assistant(result.Message), cancellationToken);
    }

    return Results.Ok(result);
});

Console.WriteLine("Jarvis UI is running at http://localhost:5055");
Console.WriteLine("CLI mode: dotnet run --project Jarvis\\Jarvis.csproj -- --cli");

await app.RunAsync();

static async Task RunCliAsync(SettingsService settingsService, OllamaService ollamaService, CommandRouter router)
{
    Console.OutputEncoding = System.Text.Encoding.UTF8;
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine("======================================");
    Console.WriteLine("        Jarvis Local AI Assistant      ");
    Console.WriteLine("======================================");
    Console.ResetColor();

    Console.WriteLine($"Ollama: {settingsService.Current.OllamaBaseUrl}");
    Console.WriteLine($"Model : {settingsService.Current.Model}");

    if (!await ollamaService.IsRunningAsync())
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("Ollama is not reachable right now. Commands still work; chat needs local Ollama running.");
        Console.ResetColor();
    }

    Console.WriteLine("Type /help for commands, /exit to quit.");

    using var cts = new CancellationTokenSource();
    Console.CancelKeyPress += (_, eventArgs) =>
    {
        eventArgs.Cancel = true;
        cts.Cancel();
    };

    while (!cts.IsCancellationRequested)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write("You > ");
        Console.ResetColor();

        var input = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(input))
        {
            continue;
        }

        if (input.Equals("/exit", StringComparison.OrdinalIgnoreCase)
            || input.Equals("exit", StringComparison.OrdinalIgnoreCase))
        {
            break;
        }

        try
        {
            await router.RouteAsync(input, cts.Token);
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Cancelled.");
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error: {ex.Message}");
            Console.ResetColor();
        }
    }

    Console.WriteLine("Goodbye.");
}

string BuildConfirmationMessage(PcCommandExecutionResult result)
{
    var target = string.IsNullOrWhiteSpace(result.Target) ? result.Command : result.Target;
    return $"Confirm {result.Command}: {target}";
}

bool TryBuildLocalAssistantResponse(string input, out string response)
{
    var normalized = input.Trim().TrimEnd('.', '!', '?').ToLowerInvariant();

    if (normalized is "diagnostics" or "check diagnostics" or "show diagnostics")
    {
        var warnings = settingsValidationService.Validate().Warnings;
        response = $"Diagnostics: platform {platformService.Current}. Memory: {pathResolver.MemoryPath}. Logs: {pathResolver.LogsDirectory}. Warnings: {(warnings.Count == 0 ? "none" : string.Join("; ", warnings))}";
        return true;
    }

    if (normalized is "voice status" or "check voice status")
    {
        response = $"Voice status: Whisper - {whisperService.StatusMessage}. Piper - {piperService.StatusMessage}. Wake word - {wakeWordService.StatusMessage}.";
        return true;
    }

    if (normalized is "search files" or "search my files")
    {
        response = "Tell me what to search for, for example: search files for resume. You can also use the Files panel for advanced local file search.";
        return true;
    }

    if (normalized.StartsWith("search files for ", StringComparison.OrdinalIgnoreCase))
    {
        var query = input.Trim()["search files for ".Length..].Trim();
        var results = fileIndexService.SearchDetailed(query, 5);
        response = results.Count == 0
            ? $"No indexed files matched \"{query}\"."
            : $"Top file matches for \"{query}\": {string.Join("; ", results.Select(result => result.RelativePath))}";
        return true;
    }

    response = string.Empty;
    return false;
}

public sealed record ChatRequest(string Message);
public sealed record ChatSessionCreateRequest(string? Title);
public sealed record ChatSessionMessageRequest(string Message);
public sealed record MemoryRequest(string Text, string? Category, string[]? Tags = null, int Importance = 3);
public sealed record MemoryUpdateRequest(string Text, string? Category, string[]? Tags = null, int? Importance = null);
public sealed record SpeakRequest(string Text);
public sealed record VoiceCommandRequest(string Transcript, bool Confirmed = false);
public sealed record WakeWordCheckRequest(string Transcript);
public sealed record FileOpenRequest(string Path);
