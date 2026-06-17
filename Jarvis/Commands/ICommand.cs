namespace Jarvis.Commands;

public interface ICommand
{
    string Name { get; }
    string Description { get; }
    string Usage { get; }

    Task ExecuteAsync(string arguments, CancellationToken cancellationToken = default);
}
