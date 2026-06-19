using Jarvis.Commands;
using Jarvis.Data;

namespace Jarvis.Core;

public sealed class CommandRouter
{
    private readonly ClassifierService _classifierService;
    private readonly CommandManager _commandManager;
    private readonly Assistant _assistant;

    public CommandRouter(ClassifierService classifierService, CommandManager commandManager, Assistant assistant)
    {
        _classifierService = classifierService;
        _commandManager = commandManager;
        _assistant = assistant;
    }

    public async Task RouteAsync(string input, CancellationToken cancellationToken = default)
    {
        if (_classifierService.LooksLikeCommand(input))
        {
            await _commandManager.TryExecuteAsync(input, cancellationToken);
            return;
        }

        await _assistant.RespondAsync(input, cancellationToken);
    }
}
