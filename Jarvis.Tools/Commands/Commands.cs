using Jarvis.Memory;
using Jarvis.Services;

namespace Jarvis.Commands;

public static class Commands
{
    public static CommandManager Create(
        MemoryService memoryService,
        SettingsService settingsService,
        FileIndexService fileIndexService,
        PcCommandService pcCommandService)
    {
        var commands = new List<ICommand>();
        var manager = new CommandManager(commands);

        commands.Add(new HelpCommand(manager));
        commands.Add(new MemoryCommand(memoryService));
        commands.Add(new SettingsCommandService(settingsService));
        commands.Add(new FileCommand(fileIndexService));
        commands.Add(new AppCommandService(pcCommandService));

        return manager;
    }
}
