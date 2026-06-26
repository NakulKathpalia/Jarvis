using Jarvis.Auth;
using Jarvis.Ingestion;
using Jarvis.Memory;
using Jarvis.Models;
using Jarvis.Security;
using Jarvis.Startup;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Jarvis.Endpoints;

public static class EndpointBootstrapper
{
    public static WebApplication MapJarvisEndpoints(this WebApplication app)
    {
        var runtime = app.Services.GetRequiredService<JarvisRuntime>();
        var pathResolver = runtime.PathResolver;
        var platformService = runtime.PlatformService;
        var settingsService = runtime.SettingsService;
        var memoryService = runtime.MemoryService;
        var knowledgeService = runtime.KnowledgeService;
        var ingestionService = runtime.IngestionService;
        var imageOcrService = runtime.ImageOcrService;
        var memoryRetrievalService = runtime.MemoryRetrievalService;
        var chatHistoryService = runtime.ChatHistoryService;
        var chatSessionService = runtime.ChatSessionService;
        var commandLogService = runtime.CommandLogService;
        var interactionLogService = runtime.InteractionLogService;
        var voiceHistoryService = runtime.VoiceHistoryService;
        var jarvisPersonalityService = runtime.JarvisPersonalityService;
        var voiceSettingsService = runtime.VoiceSettingsService;
        var speechToTextService = runtime.SpeechToTextService;
        var permissionService = runtime.PermissionService;
        var ollamaService = runtime.OllamaService;
        var whisperService = runtime.WhisperService;
        var piperService = runtime.PiperService;
        var textToSpeechService = runtime.TextToSpeechService;
        var wakeWordService = runtime.WakeWordService;
        var fileIndexService = runtime.FileIndexService;
        var pcCommandParser = runtime.PcCommandParser;
        var pcCommandService = runtime.PcCommandService;
        var voiceCommandService = runtime.VoiceCommandService;
        var settingsValidationService = runtime.SettingsValidationService;
        var authService = runtime.AuthService;
        var connectedAppService = runtime.ConnectedAppService;
        var commandManager = runtime.CommandManager;
        var assistant = runtime.Assistant;
        var voicePipelineService = runtime.VoicePipelineService;

        app.UseCors();
        app.UseDefaultFiles();
        app.UseStaticFiles();
        app.MapGet("/api/auth/status", () => Results.Ok(authService.GetStatus()));
        
        app.MapGet("/api/auth/providers", () => Results.Ok(authService.GetProviders()));
        
        app.MapPost("/api/auth/signin", (SignInRequest request) =>
        {
            return Results.Ok(authService.SignIn(request));
        });
        
        app.MapPost("/api/auth/signup", (SignUpRequest request) =>
        {
            return Results.Ok(authService.SignUp(request));
        });
        
        app.MapPost("/api/auth/signout", () => Results.Ok(authService.SignOut()));
        
        app.MapGet("/api/connected-apps", () => Results.Ok(connectedAppService.GetApps()));
        
        app.MapPost("/api/connected-apps/{provider}/connect", (string provider) =>
        {
            var denied = DenyIfMissing(PermissionDefinitions.ConnectorManageOwn);
            if (denied is not null)
            {
                return denied;
            }

            return Results.Ok(connectedAppService.Connect(provider));
        });
        
        app.MapPost("/api/connected-apps/{provider}/disconnect", (string provider) =>
        {
            var denied = DenyIfMissing(PermissionDefinitions.ConnectorManageOwn);
            if (denied is not null)
            {
                return denied;
            }

            return Results.Ok(connectedAppService.Disconnect(provider));
        });
        
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
            var ocrStatus = await imageOcrService.GetStatusAsync();
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
                validation.Warnings.Concat(ocrStatus.Available ? [] : [ocrStatus.Message]).ToList()));
        });
        
        app.MapGet("/api/history", () => Results.Ok(chatHistoryService.Messages));
        
        app.MapGet("/api/interactions/logs", (int? limit) =>
        {
            var denied = DenyIfMissing(PermissionDefinitions.AuditReadOwn);
            if (denied is not null)
            {
                return denied;
            }

            var take = Math.Clamp(limit.GetValueOrDefault(100), 1, 500);
            return Results.Ok(interactionLogService.Logs.Take(take));
        });
        
        app.MapPost("/api/interactions/logs", async (InteractionLogRequest request, CancellationToken cancellationToken) =>
        {
            await interactionLogService.AddAsync(new InteractionLogEntry
            {
                Source = request.Source,
                Type = request.Type,
                Stage = request.Stage,
                Input = request.Input ?? string.Empty,
                Output = request.Output ?? string.Empty,
                Status = request.Status,
                Message = request.Message ?? string.Empty,
                Error = request.Error ?? string.Empty,
                Metadata = request.Metadata ?? []
            }, cancellationToken);
        
            return Results.Ok(new { logged = true });
        });
        
        app.MapDelete("/api/interactions/logs", async (CancellationToken cancellationToken) =>
        {
            var denied = DenyIfMissing(PermissionDefinitions.AuditReadOwn);
            if (denied is not null)
            {
                return denied;
            }

            await interactionLogService.ClearAsync(cancellationToken);
            return Results.Ok(new { cleared = true });
        });
        
        app.MapGet("/api/interactions/status", () =>
        {
            var logs = interactionLogService.Logs;
            return Results.Ok(new InteractionStatusResult(
                true,
                logs.FirstOrDefault(),
                logs.FirstOrDefault(log => log.Source == InteractionSource.Voice && log.Type == InteractionType.Transcription),
                logs.FirstOrDefault(log => log.Type == InteractionType.CommandParsing),
                logs.FirstOrDefault(log => log.Status == InteractionStatus.Failed || log.Type == InteractionType.Error)));
        });
        
        app.MapPost("/api/chat", async (ChatRequest request, CancellationToken cancellationToken) =>
        {
            var denied = DenyIfMissing(PermissionDefinitions.ChatWrite);
            if (denied is not null)
            {
                return denied;
            }

            if (string.IsNullOrWhiteSpace(request.Message))
            {
                return Results.BadRequest(new { error = "Message is required." });
            }

            var message = request.Message.Trim();
            var parsedCommand = pcCommandParser.Parse(message);
            if (parsedCommand.Action != PcControlAction.Unknown)
            {
                var commandDenied = DenyIfMissing(PermissionDefinitions.CommandsExecute, SecurityRiskLevel.Medium);
                if (commandDenied is not null)
                {
                    return commandDenied;
                }

                var commandResult = await pcCommandService.ExecuteAsync(message, cancellationToken: cancellationToken);
                var commandResponse = commandResult.RequiresConfirmation
                    ? BuildConfirmationMessage(commandResult)
                    : commandResult.Message;
                return Results.Ok(new { response = commandResponse });
            }

            var response = await assistant.GenerateResponseAsync(
                jarvisPersonalityService.NormalizeUserInput(message),
                cancellationToken: cancellationToken);
            return Results.Ok(new { response });
        });
        
        app.MapPost("/api/assistant/input", async (AssistantInputRequest request, CancellationToken cancellationToken) =>
        {
            var chatDenied = DenyIfMissing(PermissionDefinitions.ChatWrite);
            if (chatDenied is not null)
            {
                return chatDenied;
            }

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
            await interactionLogService.AddAsync(
                InteractionSource.Chat,
                InteractionType.UserInput,
                "submitted",
                InteractionStatus.Started,
                "User submitted text.",
                message,
                cancellationToken: cancellationToken);
            await chatSessionService.AddMessageAsync(session.Id, ChatMessage.User(message), cancellationToken);
        
            if (message.StartsWith("/", StringComparison.Ordinal))
            {
                var commandDenied = DenyIfMissing(PermissionDefinitions.CommandsExecute, SecurityRiskLevel.Medium);
                if (commandDenied is not null)
                {
                    return commandDenied;
                }

                var commandResult = await commandManager.TryExecuteAsync(message, cancellationToken);
                var commandMessage = string.IsNullOrWhiteSpace(commandResult.Message)
                    ? "Command executed. Check console output."
                    : commandResult.Message;
                await chatSessionService.AddMessageAsync(session.Id, ChatMessage.Assistant(commandMessage), cancellationToken);
        
                return Results.Ok(new AssistantInputResponse(
                    "command",
                    commandResult.WasHandled,
                    false,
                    string.Empty,
                    string.Empty,
                    commandMessage,
                    null,
                    null,
                    chatSessionService.Get(session.Id)));
            }
        
            if (message.Equals(SecurityService.ConfirmationPhrase, StringComparison.Ordinal))
            {
                var commandDenied = DenyIfMissing(PermissionDefinitions.CommandsExecute, SecurityRiskLevel.Medium);
                if (commandDenied is not null)
                {
                    return commandDenied;
                }

                var commandResult = await pcCommandService.ExecuteAsync(message, cancellationToken: cancellationToken);
                await chatSessionService.AddMessageAsync(session.Id, ChatMessage.Assistant(commandResult.Message), cancellationToken);
        
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
        
            var parsedCommand = pcCommandParser.Parse(message);
            await interactionLogService.AddAsync(
                InteractionSource.Chat,
                InteractionType.CommandParsing,
                "parser-first",
                parsedCommand.Action == PcControlAction.Unknown ? InteractionStatus.Skipped : InteractionStatus.Success,
                parsedCommand.Action == PcControlAction.Unknown
                    ? "No local command detected."
                    : $"Detected command {parsedCommand.Action}.",
                message,
                parsedCommand.Target,
                cancellationToken: cancellationToken);
        
            if (parsedCommand.Action != PcControlAction.Unknown)
            {
                var commandDenied = DenyIfMissing(PermissionDefinitions.CommandsExecute, SecurityRiskLevel.Medium);
                if (commandDenied is not null)
                {
                    return commandDenied;
                }

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
        
            var normalizedMessage = jarvisPersonalityService.NormalizeUserInput(message);
            if (TryBuildLocalAssistantResponse(normalizedMessage, out var localResponse))
            {
                await chatSessionService.AddMessageAsync(session.Id, ChatMessage.Assistant(localResponse), cancellationToken);
                await interactionLogService.AddAsync(
                    InteractionSource.System,
                    InteractionType.SystemStatus,
                    "local-response",
                    InteractionStatus.Success,
                    "Local assistant status response generated.",
                    message,
                    localResponse,
                    cancellationToken: cancellationToken);
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
            await interactionLogService.AddAsync(
                InteractionSource.Chat,
                InteractionType.AiFallback,
                "fallback",
                InteractionStatus.Started,
                "Routing to Ollama.",
                message,
                cancellationToken: cancellationToken);
            var response = await assistant.GenerateSessionResponseAsync(
                normalizedMessage,
                refreshedSession?.Messages ?? [ChatMessage.User(message)],
                cancellationToken: cancellationToken);
        
            if (!string.IsNullOrWhiteSpace(response))
            {
                await chatSessionService.AddMessageAsync(session.Id, ChatMessage.Assistant(response), cancellationToken);
            }
            await interactionLogService.AddAsync(
                InteractionSource.Chat,
                InteractionType.AiResponse,
                "ollama-response",
                string.IsNullOrWhiteSpace(response) ? InteractionStatus.Failed : InteractionStatus.Success,
                "AI response completed.",
                message,
                response,
                cancellationToken: cancellationToken);
        
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
            var denied = DenyIfMissing(PermissionDefinitions.CommandsExecute, SecurityRiskLevel.Medium);
            if (denied is not null)
            {
                return denied;
            }

            if (string.IsNullOrWhiteSpace(request.ConfirmationId))
            {
                return Results.BadRequest(new { error = "Confirmation id is required." });
            }
        
            var result = await pcCommandService.ConfirmAsync(request.ConfirmationId, cancellationToken);
            await interactionLogService.AddAsync(
                InteractionSource.Chat,
                InteractionType.Confirmation,
                "accepted",
                result.Handled ? InteractionStatus.Success : InteractionStatus.Failed,
                result.Message,
                request.ConfirmationId,
                result.Target,
                cancellationToken: cancellationToken);
        
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
        
        app.MapGet("/api/chats", () =>
        {
            var denied = DenyIfMissing(PermissionDefinitions.ChatRead);
            return denied ?? Results.Ok(chatSessionService.GetSummaries());
        });
        
        app.MapGet("/api/chats/{id}", (string id) =>
        {
            var denied = DenyIfMissing(PermissionDefinitions.ChatRead);
            if (denied is not null)
            {
                return denied;
            }

            var session = chatSessionService.Get(id);
            return session is null
                ? Results.NotFound(new { error = "Chat session not found." })
                : Results.Ok(session);
        });
        
        app.MapPost("/api/chats", async (ChatSessionCreateRequest request, CancellationToken cancellationToken) =>
        {
            var denied = DenyIfMissing(PermissionDefinitions.ChatWrite);
            if (denied is not null)
            {
                return denied;
            }

            var session = await chatSessionService.CreateAsync(request.Title, cancellationToken);
            return Results.Ok(session);
        });
        
        app.MapPost("/api/chats/{id}/messages", async (string id, ChatSessionMessageRequest request, CancellationToken cancellationToken) =>
        {
            var denied = DenyIfMissing(PermissionDefinitions.ChatWrite);
            if (denied is not null)
            {
                return denied;
            }

            if (string.IsNullOrWhiteSpace(request.Message))
            {
                return Results.BadRequest(new { error = "Message is required." });
            }
        
            var session = chatSessionService.Get(id);
            if (session is null)
            {
                return Results.NotFound(new { error = "Chat session not found." });
            }
        
            var message = request.Message.Trim();
            var userMessage = ChatMessage.User(message);
            await chatSessionService.AddMessageAsync(id, userMessage, cancellationToken);

            var parsedCommand = pcCommandParser.Parse(message);
            if (parsedCommand.Action != PcControlAction.Unknown)
            {
                var commandDenied = DenyIfMissing(PermissionDefinitions.CommandsExecute, SecurityRiskLevel.Medium);
                if (commandDenied is not null)
                {
                    return commandDenied;
                }

                var commandResult = await pcCommandService.ExecuteAsync(message, cancellationToken: cancellationToken);
                var assistantMessage = commandResult.RequiresConfirmation
                    ? BuildConfirmationMessage(commandResult)
                    : commandResult.Message;

                await chatSessionService.AddMessageAsync(id, ChatMessage.Assistant(assistantMessage), cancellationToken);

                return Results.Ok(new
                {
                    response = assistantMessage,
                    session = chatSessionService.Get(id)
                });
            }
        
            var refreshedSession = chatSessionService.Get(id);
            var assistantInput = jarvisPersonalityService.NormalizeUserInput(message);
            var response = await assistant.GenerateSessionResponseAsync(
                assistantInput,
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
            var denied = DenyIfMissing(PermissionDefinitions.ChatWrite);
            if (denied is not null)
            {
                return denied;
            }

            var deleted = await chatSessionService.DeleteAsync(id, cancellationToken);
            return deleted
                ? Results.Ok(chatSessionService.GetSummaries())
                : Results.NotFound(new { error = "Chat session not found." });
        });
        
        app.MapGet("/api/memory", () =>
        {
            var denied = DenyIfMissing(PermissionDefinitions.MemoryRead);
            return denied ?? Results.Ok(memoryService.Items);
        });
        
        app.MapPost("/api/memory", async (MemoryRequest request, CancellationToken cancellationToken) =>
        {
            var denied = DenyIfMissing(PermissionDefinitions.MemoryWrite);
            if (denied is not null)
            {
                return denied;
            }

            if (string.IsNullOrWhiteSpace(request.Text))
            {
                return Results.BadRequest(new { error = "Memory text is required." });
            }
        
            await memoryService.AddAsync(
                request.Text.Trim(),
                request.Category ?? "General",
                request.Tags,
                request.Importance,
                request.Confidence,
                request.Source ?? "Manual",
                request.MemoryType,
                request.ReviewStatus,
                request.ExpiresAtUtc,
                cancellationToken);
            return Results.Ok(memoryService.Items);
        });
        
        app.MapGet("/api/memory/search", (
            string? q,
            string? category,
            string? tag,
            int? minImportance,
            int? minConfidence,
            MemoryType? memoryType,
            MemoryReviewStatus? reviewStatus,
            bool? includeExpired) =>
        {
            var denied = DenyIfMissing(PermissionDefinitions.MemoryRead);
            if (denied is not null)
            {
                return denied;
            }

            var results = memoryService.Search(
                q ?? string.Empty,
                category,
                tag,
                minImportance,
                minConfidence,
                memoryType,
                reviewStatus,
                includeExpired.GetValueOrDefault());
            return Results.Ok(results);
        });

        app.MapGet("/api/memory/suggestions", () =>
        {
            var denied = DenyIfMissing(PermissionDefinitions.MemoryRead);
            return denied ?? Results.Ok(memoryService.GetPendingSuggestions());
        });

        app.MapPost("/api/memory/retrieve", (MemoryRetrieveRequest request) =>
        {
            var denied = DenyIfMissing(PermissionDefinitions.MemoryRead);
            if (denied is not null)
            {
                return denied;
            }

            if (string.IsNullOrWhiteSpace(request.Query))
            {
                return Results.BadRequest(new { error = "Query is required." });
            }

            var options = new MemoryRetrievalOptions(
                request.MaxResults ?? settingsService.Current.MaxRetrievedMemories,
                settingsService.Current.UseTemporaryContext,
                settingsService.Current.UseSuggestedMemories);
            var memories = memoryRetrievalService.Retrieve(request.Query, options)
                .Select(result => new
                {
                    memory = result.Memory,
                    score = Math.Round(result.Score, 2),
                    matchedTerms = result.MatchedTerms,
                    matchedCategories = result.MatchedCategories
                })
                .ToList();

            return Results.Ok(new { memories });
        });

        app.MapGet("/api/memory/stats", () =>
        {
            var denied = DenyIfMissing(PermissionDefinitions.MemoryRead);
            if (denied is not null)
            {
                return denied;
            }

            var memories = memoryService.Items;
            return Results.Ok(new
            {
                total = memories.Count,
                approved = memories.Count(item => item.ReviewStatus == MemoryReviewStatus.Approved),
                pending = memories.Count(item => item.ReviewStatus == MemoryReviewStatus.Pending),
                rejected = memories.Count(item => item.ReviewStatus == MemoryReviewStatus.Rejected),
                categories = memories.GroupBy(item => item.Category)
                    .OrderByDescending(group => group.Count())
                    .Select(group => new { category = group.Key, count = group.Count() })
                    .ToList()
            });
        });

        app.MapGet("/api/knowledge", () =>
        {
            var denied = DenyIfMissing(PermissionDefinitions.MemoryRead);
            return denied ?? Results.Ok(knowledgeService.Items);
        });

        app.MapGet("/api/knowledge/stats", () =>
        {
            var denied = DenyIfMissing(PermissionDefinitions.MemoryRead);
            return denied ?? Results.Ok(knowledgeService.GetStats());
        });

        app.MapGet("/api/knowledge/search", (
            string? q,
            KnowledgeCategory? category,
            string? sourceType,
            DateTime? importedAfterUtc,
            DateTime? importedBeforeUtc,
            int? limit) =>
        {
            var denied = DenyIfMissing(PermissionDefinitions.MemoryRead);
            return denied ?? Results.Ok(knowledgeService.Search(q, category, sourceType, importedAfterUtc, importedBeforeUtc, limit ?? 50));
        });

        app.MapPost("/api/knowledge", async (KnowledgeCreateRequest request, CancellationToken cancellationToken) =>
        {
            var denied = DenyIfMissing(PermissionDefinitions.MemoryWrite, SecurityRiskLevel.Medium, "knowledge", "create");
            if (denied is not null)
            {
                return denied;
            }

            if (string.IsNullOrWhiteSpace(request.Content))
            {
                return Results.BadRequest(new { error = "Knowledge content is required." });
            }

            var item = await knowledgeService.AddAsync(
                request.Title,
                request.Content,
                request.Category,
                request.Source ?? "Manual",
                request.SourceFile ?? string.Empty,
                request.SourceType ?? string.Empty,
                cancellationToken);
            return Results.Ok(item);
        });

        app.MapDelete("/api/knowledge/{id}", async (string id, CancellationToken cancellationToken) =>
        {
            var denied = DenyIfMissing(PermissionDefinitions.MemoryWrite, SecurityRiskLevel.Medium, "knowledge", "delete");
            if (denied is not null)
            {
                return denied;
            }

            var removed = await knowledgeService.DeleteAsync(id, cancellationToken);
            return removed is null ? Results.NotFound(new { error = "Knowledge item not found." }) : Results.Ok(knowledgeService.Items);
        });
        
        app.MapPut("/api/memory/{id}", async (string id, MemoryUpdateRequest request, CancellationToken cancellationToken) =>
        {
            var denied = DenyIfMissing(PermissionDefinitions.MemoryWrite);
            if (denied is not null)
            {
                return denied;
            }

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
                request.Confidence,
                request.Source,
                request.MemoryType,
                request.ReviewStatus,
                request.ExpiresAtUtc,
                cancellationToken);
        
            return updated is null
                ? Results.NotFound(new { error = "Memory not found." })
                : Results.Ok(memoryService.Items);
        });

        app.MapPost("/api/memory/bulk-approve", async (BulkMemoryRequest request, CancellationToken cancellationToken) =>
        {
            var denied = DenyIfMissing(PermissionDefinitions.MemoryWrite, SecurityRiskLevel.Medium, "memory", "bulk-approve");
            if (denied is not null)
            {
                return denied;
            }

            var updated = await memoryService.BulkApproveAsync(request.MemoryIds, cancellationToken);
            return Results.Ok(new { updated = updated.Count, memories = memoryService.Items });
        });

        app.MapPost("/api/memory/bulk-reject", async (BulkMemoryRequest request, CancellationToken cancellationToken) =>
        {
            var denied = DenyIfMissing(PermissionDefinitions.MemoryWrite, SecurityRiskLevel.Medium, "memory", "bulk-reject");
            if (denied is not null)
            {
                return denied;
            }

            var updated = await memoryService.BulkRejectAsync(request.MemoryIds, cancellationToken);
            return Results.Ok(new { updated = updated.Count, memories = memoryService.Items });
        });

        app.MapPost("/api/memory/bulk-delete", async (BulkMemoryRequest request, CancellationToken cancellationToken) =>
        {
            var denied = DenyIfMissing(PermissionDefinitions.MemoryWrite, SecurityRiskLevel.Medium, "memory", "bulk-delete");
            if (denied is not null)
            {
                return denied;
            }

            var removed = await memoryService.BulkDeleteAsync(request.MemoryIds, cancellationToken);
            return Results.Ok(new { removed, memories = memoryService.Items });
        });

        app.MapPost("/api/memory/bulk-update", async (BulkMemoryRequest request, CancellationToken cancellationToken) =>
        {
            var denied = DenyIfMissing(PermissionDefinitions.MemoryWrite, SecurityRiskLevel.Medium, "memory", "bulk-update");
            if (denied is not null)
            {
                return denied;
            }

            var updated = await memoryService.BulkUpdateAsync(
                request.MemoryIds,
                request.Category,
                request.Importance,
                request.Confidence,
                request.ConvertSuggestedToPermanent,
                cancellationToken);
            return Results.Ok(new { updated = updated.Count, memories = memoryService.Items });
        });

        app.MapPost("/api/memory/{id}/approve", async (string id, CancellationToken cancellationToken) =>
        {
            var denied = DenyIfMissing(PermissionDefinitions.MemoryWrite);
            if (denied is not null)
            {
                return denied;
            }

            var updated = await memoryService.ApproveAsync(id, cancellationToken);
            return updated is null
                ? Results.NotFound(new { error = "Memory not found." })
                : Results.Ok(memoryService.Items);
        });

        app.MapPost("/api/memory/{id}/reject", async (string id, CancellationToken cancellationToken) =>
        {
            var denied = DenyIfMissing(PermissionDefinitions.MemoryWrite);
            if (denied is not null)
            {
                return denied;
            }

            var updated = await memoryService.RejectAsync(id, cancellationToken);
            return updated is null
                ? Results.NotFound(new { error = "Memory not found." })
                : Results.Ok(memoryService.Items);
        });
        
        app.MapDelete("/api/memory/{id}", async (string id, CancellationToken cancellationToken) =>
        {
            var denied = DenyIfMissing(PermissionDefinitions.MemoryWrite);
            if (denied is not null)
            {
                return denied;
            }

            var removed = await memoryService.DeleteAsync(id, cancellationToken);
            return removed is null
                ? Results.NotFound(new { error = "Memory not found." })
                : Results.Ok(memoryService.Items);
        });
        
        app.MapDelete("/api/memory", async (CancellationToken cancellationToken) =>
        {
            var denied = DenyIfMissing(PermissionDefinitions.MemoryWrite);
            if (denied is not null)
            {
                return denied;
            }

            await memoryService.ClearAsync(cancellationToken);
            return Results.Ok(memoryService.Items);
        });

        app.MapPost("/api/ingestion/upload", async (IFormFile file, CancellationToken cancellationToken) =>
        {
            var denied = DenyIfMissing(PermissionDefinitions.MemoryWrite, SecurityRiskLevel.Medium, "ingestion", "upload");
            if (denied is not null)
            {
                return denied;
            }

            if (file.Length <= 0)
            {
                return Results.BadRequest(new { error = "File is required." });
            }

            try
            {
                await using var stream = file.OpenReadStream();
                var job = await ingestionService.UploadAsync(file.FileName, stream, file.Length, cancellationToken);
                await interactionLogService.AddAsync(
                    InteractionSource.System,
                    InteractionType.SystemStatus,
                    "ingestion-upload",
                    InteractionStatus.Success,
                    "Ingestion upload stored locally.",
                    input: job.FileName,
                    output: job.Id,
                    cancellationToken: cancellationToken);
                return Results.Ok(job);
            }
            catch (InvalidOperationException ex)
            {
                await interactionLogService.AddAsync(
                    InteractionSource.System,
                    InteractionType.Error,
                    "ingestion-upload",
                    InteractionStatus.Failed,
                    "Ingestion upload rejected.",
                    input: file.FileName,
                    error: ex.Message,
                    cancellationToken: cancellationToken);
                return Results.BadRequest(new { error = ex.Message });
            }
        }).DisableAntiforgery();

        app.MapGet("/api/ingestion/jobs", () =>
        {
            var denied = DenyIfMissing(PermissionDefinitions.MemoryRead);
            return denied ?? Results.Ok(ingestionService.Jobs);
        });

        app.MapGet("/api/ingestion/jobs/{id}", (string id) =>
        {
            var denied = DenyIfMissing(PermissionDefinitions.MemoryRead);
            if (denied is not null)
            {
                return denied;
            }

            var job = ingestionService.Get(id);
            return job is null ? Results.NotFound(new { error = "Ingestion job not found." }) : Results.Ok(job);
        });

        app.MapGet("/api/ingestion/jobs/{id}/file", (string id) =>
        {
            var denied = DenyIfMissing(PermissionDefinitions.MemoryRead);
            if (denied is not null)
            {
                return denied;
            }

            var job = ingestionService.Get(id);
            if (job is null || !File.Exists(job.StoredPath))
            {
                return Results.NotFound(new { error = "Ingestion file not found." });
            }

            var contentType = job.SourceType == IngestionSourceType.Pdf
                ? "application/pdf"
                : job.FileType.Equals("WEBP", StringComparison.OrdinalIgnoreCase)
                    ? "image/webp"
                    : $"image/{job.FileType.ToLowerInvariant()}";
            return Results.File(job.StoredPath, contentType);
        });

        app.MapPost("/api/ingestion/jobs/{id}/extract", async (string id, CancellationToken cancellationToken) =>
        {
            var denied = DenyIfMissing(PermissionDefinitions.MemoryWrite, SecurityRiskLevel.Medium, "ingestion", "extract");
            if (denied is not null)
            {
                return denied;
            }

            var job = await ingestionService.ExtractAsync(id, cancellationToken);
            if (job is null)
            {
                return Results.NotFound(new { error = "Ingestion job not found." });
            }

            await interactionLogService.AddAsync(
                InteractionSource.System,
                job.Status == IngestionStatus.ExtractionFailed ? InteractionType.Error : InteractionType.SystemStatus,
                "ingestion-extract",
                job.Status == IngestionStatus.Extracted ? InteractionStatus.Success : InteractionStatus.Pending,
                job.Status == IngestionStatus.OcrRequired ? "OCR required for ingestion job." : "Ingestion extraction completed.",
                input: job.FileName,
                output: job.Status.ToString(),
                error: job.ErrorMessage,
                cancellationToken: cancellationToken);
            return Results.Ok(job);
        });

        app.MapPost("/api/ingestion/jobs/{id}/candidates", async (string id, CancellationToken cancellationToken) =>
        {
            var denied = DenyIfMissing(PermissionDefinitions.MemoryWrite, SecurityRiskLevel.Medium, "ingestion", "candidates");
            if (denied is not null)
            {
                return denied;
            }

            var job = await ingestionService.GenerateCandidatesAsync(id, cancellationToken);
            if (job is null)
            {
                return Results.NotFound(new { error = "Ingestion job not found." });
            }

            return Results.Ok(job);
        });

        app.MapPost("/api/ingestion/jobs/{id}/knowledge", async (
            string id,
            SaveIngestionAsKnowledgeRequest request,
            CancellationToken cancellationToken) =>
        {
            var denied = DenyIfMissing(PermissionDefinitions.MemoryWrite, SecurityRiskLevel.Medium, "ingestion", "save-knowledge");
            if (denied is not null)
            {
                return denied;
            }

            var job = ingestionService.Get(id);
            if (job is null)
            {
                return Results.NotFound(new { error = "Ingestion job not found." });
            }

            if (string.IsNullOrWhiteSpace(job.ExtractedText))
            {
                return Results.BadRequest(new { error = "Extracted text is required before saving as knowledge." });
            }

            var item = await knowledgeService.AddAsync(
                request.Title ?? Path.GetFileNameWithoutExtension(job.FileName),
                job.ExtractedText,
                request.Category,
                $"Ingestion:{job.ExtractionSource}",
                job.FileName,
                job.SourceType.ToString(),
                cancellationToken);
            return Results.Ok(new { knowledge = item, items = knowledgeService.Items });
        });

        app.MapPost("/api/ingestion/jobs/{id}/memory", async (
            string id,
            SaveIngestionAsMemoryRequest request,
            CancellationToken cancellationToken) =>
        {
            var denied = DenyIfMissing(PermissionDefinitions.MemoryWrite, SecurityRiskLevel.Medium, "ingestion", "save-memory");
            if (denied is not null)
            {
                return denied;
            }

            var job = ingestionService.Get(id);
            if (job is null)
            {
                return Results.NotFound(new { error = "Ingestion job not found." });
            }

            var text = string.IsNullOrWhiteSpace(request.Text) ? job.ExtractedText : request.Text;
            if (string.IsNullOrWhiteSpace(text))
            {
                return Results.BadRequest(new { error = "Extracted text is required before saving as memory." });
            }

            var memory = await memoryService.AddAsync(
                text,
                request.Category ?? "General",
                tags: ["ingested"],
                importance: request.Importance,
                confidence: request.Confidence,
                source: $"Ingestion:{job.FileName}",
                memoryType: request.MemoryType,
                reviewStatus: MemoryReviewStatus.Approved,
                cancellationToken: cancellationToken);
            return Results.Ok(new { memory, memories = memoryService.Items });
        });

        app.MapDelete("/api/ingestion/jobs/{id}", async (string id, CancellationToken cancellationToken) =>
        {
            var denied = DenyIfMissing(PermissionDefinitions.MemoryWrite, SecurityRiskLevel.Medium, "ingestion", "delete");
            if (denied is not null)
            {
                return denied;
            }

            var removed = await ingestionService.DeleteAsync(id, cancellationToken);
            if (removed is null)
            {
                return Results.NotFound(new { error = "Ingestion job not found." });
            }

            await interactionLogService.AddAsync(
                InteractionSource.System,
                InteractionType.SystemStatus,
                "ingestion-delete",
                InteractionStatus.Success,
                "Ingestion job deleted.",
                input: removed.FileName,
                output: removed.Id,
                cancellationToken: cancellationToken);
            return Results.Ok(ingestionService.Jobs);
        });

        app.MapPost("/api/ingestion/candidates/{id}/approve", async (
            string id,
            IngestionCandidateUpdateRequest request,
            CancellationToken cancellationToken) =>
        {
            var denied = DenyIfMissing(PermissionDefinitions.MemoryWrite, SecurityRiskLevel.Medium, "ingestion", "approve");
            if (denied is not null)
            {
                return denied;
            }

            var (job, candidate, memory) = await ingestionService.ApproveCandidateAsync(
                id,
                request.Content,
                request.Category,
                request.MemoryType,
                request.Importance,
                request.Confidence,
                cancellationToken);
            if (job is null || candidate is null || memory is null)
            {
                return Results.NotFound(new { error = "Ingestion candidate not found." });
            }

            await interactionLogService.AddAsync(
                InteractionSource.System,
                InteractionType.SystemStatus,
                "ingestion-candidate-approve",
                InteractionStatus.Success,
                "Ingestion memory candidate approved.",
                input: candidate.SourceFile,
                output: memory.Id,
                cancellationToken: cancellationToken);
            return Results.Ok(new { job, candidate, memory, memories = memoryService.Items });
        });

        app.MapPost("/api/ingestion/candidates/bulk-approve", async (
            BulkIngestionCandidateRequest request,
            CancellationToken cancellationToken) =>
        {
            var denied = DenyIfMissing(PermissionDefinitions.MemoryWrite, SecurityRiskLevel.Medium, "ingestion", "bulk-approve");
            if (denied is not null)
            {
                return denied;
            }

            var results = await ingestionService.ApproveCandidatesAsync(
                request.CandidateIds,
                request.Category,
                request.Importance,
                request.Confidence,
                cancellationToken);
            return Results.Ok(new
            {
                approved = results.Count(result => result.Memory is not null),
                jobs = ingestionService.Jobs,
                memories = memoryService.Items
            });
        });

        app.MapPost("/api/ingestion/candidates/{id}/reject", async (string id, CancellationToken cancellationToken) =>
        {
            var denied = DenyIfMissing(PermissionDefinitions.MemoryWrite, SecurityRiskLevel.Medium, "ingestion", "reject");
            if (denied is not null)
            {
                return denied;
            }

            var (job, candidate) = await ingestionService.RejectCandidateAsync(id, cancellationToken);
            if (job is null || candidate is null)
            {
                return Results.NotFound(new { error = "Ingestion candidate not found." });
            }

            await interactionLogService.AddAsync(
                InteractionSource.System,
                InteractionType.SystemStatus,
                "ingestion-candidate-reject",
                InteractionStatus.Success,
                "Ingestion memory candidate rejected.",
                input: candidate.SourceFile,
                output: candidate.Id,
                cancellationToken: cancellationToken);
            return Results.Ok(new { job, candidate });
        });

        app.MapPost("/api/ingestion/candidates/bulk-reject", async (
            BulkIngestionCandidateRequest request,
            CancellationToken cancellationToken) =>
        {
            var denied = DenyIfMissing(PermissionDefinitions.MemoryWrite, SecurityRiskLevel.Medium, "ingestion", "bulk-reject");
            if (denied is not null)
            {
                return denied;
            }

            var results = await ingestionService.RejectCandidatesAsync(request.CandidateIds, cancellationToken);
            return Results.Ok(new
            {
                rejected = results.Count(result => result.Candidate is not null),
                jobs = ingestionService.Jobs
            });
        });
        
        app.MapGet("/api/settings", () =>
        {
            var denied = DenyIfMissing(PermissionDefinitions.SettingsRead);
            return denied ?? Results.Ok(settingsService.Current);
        });
        
        app.MapPost("/api/settings", async (AppSettings request, CancellationToken cancellationToken) =>
        {
            var denied = DenyIfMissing(PermissionDefinitions.SettingsWriteOwn, SecurityRiskLevel.Medium);
            if (denied is not null)
            {
                return denied;
            }

            settingsService.Current.OllamaBaseUrl = request.OllamaBaseUrl;
            settingsService.Current.Model = request.Model;
            settingsService.Current.OllamaContextLength = Math.Clamp(
                request.OllamaContextLength <= 0 ? AppSettings.DefaultOllamaContextLength : request.OllamaContextLength,
                512,
                32768);
            settingsService.Current.SystemPrompt = request.SystemPrompt;
            settingsService.Current.MaxHistoryMessages = Math.Max(1, request.MaxHistoryMessages);
            settingsService.Current.MemoryRetrievalEnabled = request.MemoryRetrievalEnabled;
            settingsService.Current.MaxRetrievedMemories = Math.Clamp(request.MaxRetrievedMemories, 1, 10);
            settingsService.Current.UseTemporaryContext = request.UseTemporaryContext;
            settingsService.Current.UseSuggestedMemories = request.UseSuggestedMemories;
            settingsService.Current.FileIndexRoot = request.FileIndexRoot;
            settingsService.Current.VoiceMode = request.VoiceMode;
            settingsService.Current.AutoExecuteCommands = request.AutoExecuteCommands;
            settingsService.Current.VoiceLanguage = request.VoiceLanguage;
            settingsService.Current.NoiseSuppression = request.NoiseSuppression;
            settingsService.Current.WhisperExecutablePath = request.WhisperExecutablePath;
            settingsService.Current.WhisperModelPath = request.WhisperModelPath;
            settingsService.Current.WhisperLanguage = request.WhisperLanguage;
            settingsService.Current.PiperExecutablePath = request.PiperExecutablePath;
            settingsService.Current.PiperModelPath = request.PiperModelPath;
            settingsService.Current.TesseractExecutablePath = request.TesseractExecutablePath;
            settingsService.Current.TesseractLanguage = string.IsNullOrWhiteSpace(request.TesseractLanguage)
                ? "eng+hin+san"
                : request.TesseractLanguage.Trim();
            settingsService.Current.EnableVoiceResponses = request.EnableVoiceResponses;
            settingsService.Current.VoiceName = request.VoiceName;
            settingsService.Current.SpeechRate = Math.Clamp(request.SpeechRate <= 0 ? 1.0 : request.SpeechRate, 0.5, 2.0);
            settingsService.Current.SpeechVolume = Math.Clamp(request.SpeechVolume <= 0 ? 1.0 : request.SpeechVolume, 0.0, 1.0);
            settingsService.Current.AutoSpeakAssistantReplies = request.AutoSpeakAssistantReplies;
            settingsService.Current.AutoSpeakResponses = request.AutoSpeakResponses;
            settingsService.Current.ResponseStyle = request.ResponseStyle;
            settingsService.Current.PreferredLanguage = request.PreferredLanguage;
            settingsService.Current.Verbosity = request.Verbosity;
            settingsService.Current.WakeWordEnabled = request.WakeWordEnabled;
            settingsService.Current.WakeWordPhrase = request.WakeWordPhrase;
            settingsService.Current.WakeWordDetectorPath = request.WakeWordDetectorPath;
            settingsService.Current.WakeWordModelPath = request.WakeWordModelPath;
        
            await settingsService.SaveAsync(cancellationToken);
            return Results.Ok(settingsService.Current);
        });
        
        app.MapPost("/api/files/index", async (CancellationToken cancellationToken) =>
        {
            var denied = DenyIfMissing(PermissionDefinitions.FilesRead, SecurityRiskLevel.Medium);
            if (denied is not null)
            {
                return denied;
            }

            var count = await fileIndexService.RebuildAsync(cancellationToken);
            return Results.Ok(new { count });
        });
        
        app.MapGet("/api/files/search", (string q) =>
        {
            var denied = DenyIfMissing(PermissionDefinitions.FilesRead, SecurityRiskLevel.Medium);
            if (denied is not null)
            {
                return denied;
            }

            return Results.Ok(fileIndexService.Search(q));
        });
        
        app.MapGet("/api/files/search-detailed", (string q, int? limit) =>
        {
            var denied = DenyIfMissing(PermissionDefinitions.FilesRead, SecurityRiskLevel.Medium);
            if (denied is not null)
            {
                return denied;
            }

            var results = fileIndexService.SearchDetailed(q, limit.GetValueOrDefault(25));
            return Results.Ok(results);
        });
        
        app.MapPost("/api/files/open", (FileOpenRequest request) =>
        {
            var denied = DenyIfMissing(PermissionDefinitions.FilesRead, SecurityRiskLevel.Medium);
            if (denied is not null)
            {
                return denied;
            }

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
            var denied = DenyIfMissing(PermissionDefinitions.FilesRead, SecurityRiskLevel.Medium);
            if (denied is not null)
            {
                return denied;
            }

            if (string.IsNullOrWhiteSpace(request.Path))
            {
                return Results.BadRequest(new { error = "Path is required." });
            }
        
            var success = fileIndexService.OpenContainingFolder(request.Path);
            return success
                ? Results.Ok(new { success = true, message = "Folder opened." })
                : Results.BadRequest(new { error = "Unable to open folder." });
        });
        
        app.MapGet("/api/commands/catalog", () =>
        {
            var denied = DenyIfMissing(PermissionDefinitions.CommandsExecute, SecurityRiskLevel.Medium);
            return denied ?? Results.Ok(pcCommandService.Catalog);
        });
        
        app.MapGet("/api/commands/logs", () =>
        {
            var denied = DenyIfMissing(PermissionDefinitions.AuditReadOwn);
            return denied ?? Results.Ok(commandLogService.Logs.Take(50));
        });
        
        app.MapPost("/api/commands/execute", async (PcCommandExecuteRequest request, CancellationToken cancellationToken) =>
        {
            var denied = DenyIfMissing(PermissionDefinitions.CommandsExecute, SecurityRiskLevel.Medium);
            if (denied is not null)
            {
                return denied;
            }

            if (string.IsNullOrWhiteSpace(request.Input))
            {
                return Results.BadRequest(new { error = "Command input is required." });
            }
        
            var result = await pcCommandService.ExecuteAsync(request.Input.Trim(), cancellationToken: cancellationToken);
            return Results.Ok(result);
        });
        
        app.MapPost("/api/commands/confirm", async (PcCommandConfirmRequest request, CancellationToken cancellationToken) =>
        {
            var denied = DenyIfMissing(PermissionDefinitions.CommandsExecute, SecurityRiskLevel.Medium);
            if (denied is not null)
            {
                return denied;
            }

            if (string.IsNullOrWhiteSpace(request.ConfirmationId))
            {
                return Results.BadRequest(new { error = "Confirmation id is required." });
            }
        
            var result = await pcCommandService.ConfirmAsync(request.ConfirmationId.Trim(), cancellationToken);
            await interactionLogService.AddAsync(
                InteractionSource.Control,
                InteractionType.Confirmation,
                "accepted",
                result.Handled ? InteractionStatus.Success : InteractionStatus.Failed,
                result.Message,
                request.ConfirmationId,
                result.Target,
                cancellationToken: cancellationToken);
            return Results.Ok(result);
        });
        
        app.MapGet("/api/voice/status", () =>
        {
            var denied = DenyIfMissing(PermissionDefinitions.VoiceUse);
            if (denied is not null)
            {
                return denied;
            }

            var ocrStatus = imageOcrService.GetStatusAsync().GetAwaiter().GetResult();
            return Results.Ok(new
        {
            mode = settingsService.Current.VoiceMode,
            implementedMode = "PushToTalk",
            autoExecuteCommands = settingsService.Current.AutoExecuteCommands,
            voiceLanguage = voiceSettingsService.Language,
            noiseSuppression = settingsService.Current.NoiseSuppression,
            whisper = new
            {
                configured = speechToTextService.IsConfigured,
                message = speechToTextService.StatusMessage,
                settingsService.Current.WhisperExecutablePath,
                settingsService.Current.WhisperModelPath,
                settingsService.Current.WhisperLanguage,
                settingsService.Current.VoiceLanguage,
                engine = speechToTextService.EngineName,
                preferredDevice = speechToTextService.PreferredDevice,
                fallbackDevice = speechToTextService.FallbackDevice
            },
            piper = new
            {
                configured = piperService.IsConfigured,
                message = piperService.StatusMessage,
                settingsService.Current.PiperExecutablePath,
                settingsService.Current.PiperModelPath,
                settingsService.Current.AutoSpeakResponses
            },
            ocr = new
            {
                available = ocrStatus.Available,
                configured = ocrStatus.Available,
                status = ocrStatus.Status,
                message = ocrStatus.Message,
                executablePath = ocrStatus.ExecutablePath,
                language = ocrStatus.Language,
                hindiAvailable = ocrStatus.HindiAvailable,
                installedLanguages = ocrStatus.InstalledLanguages,
                settingsService.Current.TesseractExecutablePath,
                settingsService.Current.TesseractLanguage
            },
            tts = new
            {
                enabled = settingsService.Current.EnableVoiceResponses,
                available = textToSpeechService.IsAvailable,
                provider = textToSpeechService.Provider,
                voiceName = textToSpeechService.VoiceName,
                speechRate = settingsService.Current.SpeechRate,
                speechVolume = settingsService.Current.SpeechVolume,
                autoSpeakAssistantReplies = settingsService.Current.AutoSpeakAssistantReplies,
                message = textToSpeechService.StatusMessage
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
        });
        });

        app.MapGet("/api/voice/health", async (CancellationToken cancellationToken) =>
        {
            var denied = DenyIfMissing(PermissionDefinitions.VoiceUse);
            if (denied is not null)
            {
                return denied;
            }

            var ollamaRunning = await ollamaService.IsRunningAsync(cancellationToken);
            var ocrStatus = await imageOcrService.GetStatusAsync(cancellationToken);
            return Results.Ok(new
            {
                microphone = new
                {
                    status = "BrowserPermissionRequired",
                    message = "Microphone permission is checked in the browser when Start Listening is pressed."
                },
                audioCapture = new
                {
                    status = "Ready",
                    message = "Push-to-talk capture is available in the web client."
                },
                whisper = new
                {
                    available = speechToTextService.IsConfigured,
                    message = speechToTextService.StatusMessage,
                    preferredDevice = speechToTextService.PreferredDevice,
                    fallbackDevice = speechToTextService.FallbackDevice,
                    mode = speechToTextService.IsConfigured
                        ? $"{speechToTextService.EngineName} {speechToTextService.PreferredDevice} preferred with {speechToTextService.FallbackDevice} fallback"
                        : "Not configured"
                },
                ollama = new
                {
                    available = ollamaRunning,
                    message = ollamaRunning ? "Ollama is reachable." : "Ollama is not reachable."
                },
                tts = new
                {
                    available = textToSpeechService.IsAvailable,
                    provider = textToSpeechService.Provider,
                    voiceName = textToSpeechService.VoiceName,
                    playbackCapability = textToSpeechService.IsAvailable ? "Generated audio playback through browser" : "Unavailable",
                    message = textToSpeechService.StatusMessage
                },
                ocr = new
                {
                    available = ocrStatus.Available,
                    status = ocrStatus.Status,
                    message = ocrStatus.Message,
                    executablePath = ocrStatus.ExecutablePath,
                    language = ocrStatus.Language,
                    hindiAvailable = ocrStatus.HindiAvailable,
                    installedLanguages = ocrStatus.InstalledLanguages
                },
                voiceService = new
                {
                    status = voicePipelineService.Status.State,
                    message = voicePipelineService.Status.Message,
                    lastCompletedStage = voicePipelineService.Status.LastCompletedStage,
                    errorDetails = voicePipelineService.Status.ErrorDetails,
                    audioDuration = voicePipelineService.Status.SpeechDurationMs,
                    playbackSuccess = voicePipelineService.Status.PlaybackReady,
                    playbackFailureReason = voicePipelineService.Status.PlaybackFailureReason
                }
            });
        });

        app.MapGet("/api/voice/diagnostics", () =>
        {
            var denied = DenyIfMissing(PermissionDefinitions.VoiceUse);
            if (denied is not null)
            {
                return denied;
            }

            return Results.Ok(new
            {
                status = voicePipelineService.Status,
                recentHistory = voiceHistoryService.Items.Take(10),
                recentLogs = interactionLogService.Logs
                    .Where(log => log.Source == InteractionSource.Voice)
                    .Take(25)
            });
        });
        
        app.MapGet("/api/voice/wake-status", () =>
        {
            var denied = DenyIfMissing(PermissionDefinitions.VoiceUse);
            if (denied is not null)
            {
                return denied;
            }

            return Results.Ok(new
        {
            enabled = settingsService.Current.WakeWordEnabled,
            configured = wakeWordService.IsConfigured,
            mode = wakeWordService.Mode,
            message = wakeWordService.StatusMessage,
            settingsService.Current.WakeWordPhrase,
            settingsService.Current.WakeWordDetectorPath,
            settingsService.Current.WakeWordModelPath
        });
        });
        
        app.MapPost("/api/voice/wake-check", (WakeWordCheckRequest request) =>
        {
            var denied = DenyIfMissing(PermissionDefinitions.VoiceUse);
            if (denied is not null)
            {
                return denied;
            }

            if (string.IsNullOrWhiteSpace(request.Transcript))
            {
                return Results.BadRequest(new { error = "Transcript is required." });
            }
        
            return Results.Ok(wakeWordService.CheckTranscript(request.Transcript));
        });
        
        app.MapGet("/api/voice/commands", () =>
        {
            var denied = DenyIfMissing(PermissionDefinitions.VoiceUse);
            return denied ?? Results.Ok(voiceCommandService.GetCatalog());
        });
        
        app.MapGet("/api/voice/tts-status", () =>
        {
            var denied = DenyIfMissing(PermissionDefinitions.VoiceUse);
            if (denied is not null)
            {
                return denied;
            }

            return Results.Ok(new
        {
            configured = textToSpeechService.IsAvailable,
            enabled = settingsService.Current.EnableVoiceResponses,
            provider = textToSpeechService.Provider,
            voiceName = textToSpeechService.VoiceName,
            speechRate = settingsService.Current.SpeechRate,
            speechVolume = settingsService.Current.SpeechVolume,
            autoSpeakAssistantReplies = settingsService.Current.AutoSpeakAssistantReplies,
            message = textToSpeechService.StatusMessage,
            settingsService.Current.PiperExecutablePath,
            settingsService.Current.PiperModelPath,
            settingsService.Current.AutoSpeakResponses
        });
        });
        
        app.MapPost("/api/voice/transcribe", async (HttpRequest request, CancellationToken cancellationToken) =>
        {
            var denied = DenyIfMissing(PermissionDefinitions.VoiceUse);
            if (denied is not null)
            {
                return denied;
            }

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
        
            await interactionLogService.AddAsync(
                InteractionSource.Voice,
                InteractionType.Transcription,
                "whisper-start",
                InteractionStatus.Started,
                "Audio uploaded for transcription.",
                audio.FileName,
                $"{audio.Length} bytes",
                cancellationToken: cancellationToken);
            var result = await whisperService.TranscribeAsync(audio, cancellationToken);
            await interactionLogService.AddAsync(
                InteractionSource.Voice,
                InteractionType.Transcription,
                "whisper-result",
                result.Succeeded ? InteractionStatus.Success : InteractionStatus.Failed,
                result.Message,
                audio.FileName,
                result.Transcript,
                result.Succeeded ? string.Empty : result.Message,
                cancellationToken: cancellationToken);
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
        
        app.MapGet("/api/voice/pipeline/status", () =>
        {
            var denied = DenyIfMissing(PermissionDefinitions.VoiceUse);
            return denied ?? Results.Ok(voicePipelineService.Status);
        });
        
        app.MapGet("/api/voice/history", () =>
        {
            var denied = DenyIfMissing(PermissionDefinitions.VoiceUse);
            return denied ?? Results.Ok(voiceHistoryService.Items.Take(50));
        });
        
        app.MapPost("/api/voice/pipeline", async (HttpRequest request, CancellationToken cancellationToken) =>
        {
            var denied = DenyIfMissing(PermissionDefinitions.VoiceUse);
            if (denied is not null)
            {
                return denied;
            }

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
        
            await interactionLogService.AddAsync(
                InteractionSource.Voice,
                InteractionType.VoiceRecording,
                "audio-uploaded",
                InteractionStatus.Success,
                "Voice pipeline audio uploaded.",
                audio.FileName,
                $"{audio.Length} bytes",
                cancellationToken: cancellationToken);
            var result = await voicePipelineService.ProcessAsync(audio, requireWakeWord, cancellationToken);
            await interactionLogService.AddAsync(
                InteractionSource.Voice,
                result.CommandDetected ? InteractionType.CommandExecution : InteractionType.AiFallback,
                result.State.ToString(),
                result.Success ? InteractionStatus.Success : InteractionStatus.Failed,
                result.Message,
                result.Transcript,
                result.CommandDetected ? result.CommandName : result.AiResponse,
                result.Success ? string.Empty : result.Message,
                cancellationToken: cancellationToken);
            return Results.Ok(result);
        });
        
        app.MapPost("/api/voice/pipeline/confirm", async (VoiceConfirmationRequest request, CancellationToken cancellationToken) =>
        {
            var denied = DenyIfMissing(PermissionDefinitions.VoiceUse);
            if (denied is not null)
            {
                return denied;
            }

            if (string.IsNullOrWhiteSpace(request.ConfirmationId))
            {
                return Results.BadRequest(new { error = "Confirmation id is required." });
            }
        
            var result = await voicePipelineService.ConfirmAsync(request.ConfirmationId.Trim(), cancellationToken);
            await interactionLogService.AddAsync(
                InteractionSource.Voice,
                InteractionType.Confirmation,
                "accepted",
                result.Success ? InteractionStatus.Success : InteractionStatus.Failed,
                result.Message,
                request.ConfirmationId,
                result.CommandName,
                result.Success ? string.Empty : result.Message,
                cancellationToken: cancellationToken);
            return Results.Ok(result);
        });
        
        app.MapPost("/api/voice/speak", async (SpeakRequest request, CancellationToken cancellationToken) =>
        {
            var denied = DenyIfMissing(PermissionDefinitions.VoiceUse);
            if (denied is not null)
            {
                return denied;
            }

            if (string.IsNullOrWhiteSpace(request.Text))
            {
                return Results.BadRequest(new { error = "Text is required." });
            }
        
            var text = request.Text.Trim();
            await interactionLogService.AddAsync(
                InteractionSource.Voice,
                InteractionType.Tts,
                "tts-start",
                InteractionStatus.Started,
                "Generating spoken response.",
                text,
                cancellationToken: cancellationToken);
            var result = await textToSpeechService.SpeakAsync(text, cancellationToken);
            await interactionLogService.AddAsync(
                InteractionSource.Voice,
                InteractionType.Tts,
                "tts-result",
                result.Succeeded ? InteractionStatus.Success : InteractionStatus.Failed,
                result.Message,
                text,
                result.AudioUrl,
                result.Succeeded ? string.Empty : result.Message,
                cancellationToken: cancellationToken);
            return Results.Ok(new
            {
                audioUrl = result.AudioUrl,
                ready = result.IsReady,
                succeeded = result.Succeeded,
                message = result.Message,
                provider = result.Provider,
                voiceName = result.VoiceName,
                spokenText = result.SpokenText,
                speechDurationMs = result.SpeechDurationMs,
                failureReason = result.FailureReason
            });
        });

        app.MapPost("/api/voice/speak/stop", async (CancellationToken cancellationToken) =>
        {
            var denied = DenyIfMissing(PermissionDefinitions.VoiceUse);
            if (denied is not null)
            {
                return denied;
            }

            await textToSpeechService.StopAsync(cancellationToken);
            await interactionLogService.AddAsync(
                InteractionSource.Voice,
                InteractionType.Tts,
                "tts-stop",
                InteractionStatus.Success,
                "Speech playback stop requested.",
                cancellationToken: cancellationToken);
            return Results.Ok(new { stopped = true, message = "Speech playback stop requested." });
        });
        
        app.MapPost("/api/voice/command", async (VoiceCommandRequest request, CancellationToken cancellationToken) =>
        {
            var denied = DenyIfMissing(PermissionDefinitions.VoiceUse);
            if (denied is not null)
            {
                return denied;
            }

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

        string BuildConfirmationMessage(PcCommandExecutionResult result)
        {
            var target = string.IsNullOrWhiteSpace(result.Target) ? result.Command : result.Target;
            return $"Ji sir, pehle confirmation chahiye: {target}.";
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

            if (normalized is "show memories" or "show memory" or "list memories" or "list memory")
            {
                var now = DateTime.UtcNow;
                var approvedMemories = memoryService.Items
                    .Where(memory => memory.ReviewStatus == MemoryReviewStatus.Approved
                        && (memory.MemoryType != MemoryType.TemporaryContext
                            || !memory.ExpiresAtUtc.HasValue
                            || memory.ExpiresAtUtc.Value > now))
                    .Take(8)
                    .Select(memory => memory.Text.Trim())
                    .Where(text => !string.IsNullOrWhiteSpace(text))
                    .ToList();
                response = approvedMemories.Count == 0
                    ? "Abhi koi approved memory saved nahi hai."
                    : $"Mujhe yaad hai: {string.Join("; ", approvedMemories)}";
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

        IResult? DenyIfMissing(
            string permission,
            SecurityRiskLevel riskLevel = SecurityRiskLevel.Safe,
            string resource = "",
            string action = "")
        {
            var result = permissionService.Evaluate(permission, riskLevel, resource, action);
            return result.Decision == PermissionDecision.Deny
                ? Results.Problem(result.Reason, statusCode: StatusCodes.Status403Forbidden)
                : null;
        }

        return app;
    }
}
