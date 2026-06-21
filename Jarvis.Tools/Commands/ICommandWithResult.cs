namespace Jarvis.Commands;

public interface ICommandWithResult : ICommand
{
    Task<string> ExecuteWithResultAsync(string arguments, CancellationToken cancellationToken = default);
}
