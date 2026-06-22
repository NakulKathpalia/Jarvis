using System.Text;
using Jarvis.Data;
using Jarvis.Memory;
using Jarvis.Models;
using Jarvis.Services;

namespace Jarvis.Core;

public sealed class Assistant
{
    private readonly OllamaService _ollamaService;
    private readonly SettingsService _settingsService;
    private readonly MemoryService _memoryService;
    private readonly MemoryRetrievalService _memoryRetrievalService;
    private readonly MemoryContextBuilder _memoryContextBuilder;
    private readonly ChatHistoryService _chatHistoryService;
    private readonly JarvisPersonalityService _personalityService;

    public Assistant(
        OllamaService ollamaService,
        SettingsService settingsService,
        MemoryService memoryService,
        ChatHistoryService chatHistoryService,
        JarvisPersonalityService personalityService,
        MemoryRetrievalService? memoryRetrievalService = null,
        MemoryContextBuilder? memoryContextBuilder = null)
    {
        _ollamaService = ollamaService;
        _settingsService = settingsService;
        _memoryService = memoryService;
        _memoryRetrievalService = memoryRetrievalService ?? new MemoryRetrievalService(memoryService);
        _memoryContextBuilder = memoryContextBuilder ?? new MemoryContextBuilder();
        _chatHistoryService = chatHistoryService;
        _personalityService = personalityService;
    }

    public async Task RespondAsync(string input, CancellationToken cancellationToken = default)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write("Jarvis > ");
        Console.ResetColor();

        var response = await GenerateResponseAsync(input, chunk =>
        {
            Console.Write(chunk);
            return Task.CompletedTask;
        }, cancellationToken);

        Console.WriteLine();

        if (string.IsNullOrWhiteSpace(response))
        {
            Console.WriteLine("(empty response)");
        }
    }

    public async Task<string> GenerateResponseAsync(
        string input,
        Func<string, Task>? onChunk = null,
        CancellationToken cancellationToken = default)
    {
        await _chatHistoryService.AddAsync(ChatMessage.User(input), cancellationToken);

        var payload = BuildPrompt(input);
        var responseBuilder = new StringBuilder();
        await foreach (var chunk in _ollamaService.StreamChatAsync(payload, cancellationToken))
        {
            if (onChunk is not null)
            {
                await onChunk(chunk);
            }

            responseBuilder.Append(chunk);
        }

        var response = responseBuilder.ToString();
        if (!string.IsNullOrWhiteSpace(response))
        {
            await _chatHistoryService.AddAsync(ChatMessage.Assistant(response), cancellationToken);
        }

        return response;
    }

    public Task<string> GenerateSessionResponseAsync(
        string input,
        IEnumerable<ChatMessage> sessionMessages,
        Func<string, Task>? onChunk = null,
        CancellationToken cancellationToken = default)
    {
        var prompt = BuildPrompt(input, sessionMessages);
        return GenerateFromPromptAsync(prompt, onChunk, cancellationToken);
    }

    private IEnumerable<ChatMessage> BuildPrompt(string input)
    {
        return BuildPrompt(input, _chatHistoryService.GetRecent(_settingsService.Current.MaxHistoryMessages));
    }

    private IEnumerable<ChatMessage> BuildPrompt(string input, IEnumerable<ChatMessage> history)
    {
        var systemPrompt = new StringBuilder();
        var normalizedInput = _personalityService.NormalizeUserInput(input);
        systemPrompt.AppendLine(_settingsService.Current.SystemPrompt);
        systemPrompt.AppendLine(_personalityService.BuildSystemInstructions(normalizedInput));

        if (_settingsService.Current.MemoryRetrievalEnabled)
        {
            var retrievalOptions = new MemoryRetrievalOptions(
                _settingsService.Current.MaxRetrievedMemories,
                _settingsService.Current.UseTemporaryContext,
                _settingsService.Current.UseSuggestedMemories);
            var relevantMemories = _memoryRetrievalService.Retrieve(normalizedInput, retrievalOptions);
            var memoryContext = _memoryContextBuilder.Build(relevantMemories, retrievalOptions.ClampedMaxResults);

            if (!string.IsNullOrWhiteSpace(memoryContext))
            {
                systemPrompt.AppendLine();
                systemPrompt.AppendLine(memoryContext);
                systemPrompt.AppendLine("Use these memories only as private context. Do not expose unrelated memories or mention this context block unless it helps answer the user.");
            }
        }

        yield return ChatMessage.System(systemPrompt.ToString());

        foreach (var message in history
            .Where(message => message.Role is "user" or "assistant")
            .TakeLast(_settingsService.Current.MaxHistoryMessages))
        {
            yield return message;
        }

        if (!normalizedInput.Equals(input, StringComparison.Ordinal))
        {
            yield return ChatMessage.User(normalizedInput);
        }
    }

    private async Task<string> GenerateFromPromptAsync(
        IEnumerable<ChatMessage> prompt,
        Func<string, Task>? onChunk,
        CancellationToken cancellationToken)
    {
        var responseBuilder = new StringBuilder();
        await foreach (var chunk in _ollamaService.StreamChatAsync(prompt, cancellationToken))
        {
            if (onChunk is not null)
            {
                await onChunk(chunk);
            }

            responseBuilder.Append(chunk);
        }

        return responseBuilder.ToString();
    }
}
