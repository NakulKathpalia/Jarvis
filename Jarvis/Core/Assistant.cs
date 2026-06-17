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
    private readonly ChatHistoryService _chatHistoryService;

    public Assistant(
        OllamaService ollamaService,
        SettingsService settingsService,
        MemoryService memoryService,
        ChatHistoryService chatHistoryService)
    {
        _ollamaService = ollamaService;
        _settingsService = settingsService;
        _memoryService = memoryService;
        _chatHistoryService = chatHistoryService;
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

        var payload = BuildPrompt();
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

    private IEnumerable<ChatMessage> BuildPrompt()
    {
        var systemPrompt = new StringBuilder();
        systemPrompt.AppendLine(_settingsService.Current.SystemPrompt);

        if (_memoryService.Items.Count > 0)
        {
            systemPrompt.AppendLine();
            systemPrompt.AppendLine("Known local memory:");
            foreach (var item in _memoryService.Items.Take(20))
            {
                systemPrompt.AppendLine($"- {item.Text}");
            }
        }

        yield return ChatMessage.System(systemPrompt.ToString());

        foreach (var message in _chatHistoryService.GetRecent(_settingsService.Current.MaxHistoryMessages))
        {
            yield return message;
        }
    }
}
