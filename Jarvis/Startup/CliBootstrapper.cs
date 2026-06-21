using Jarvis.Commands;
using Jarvis.Core;
using Jarvis.Services;

namespace Jarvis.Startup;

public static class CliBootstrapper
{
    public static async Task RunAsync(SettingsService settingsService, OllamaService ollamaService, CommandRouter router)
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
}
