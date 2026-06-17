namespace Jarvis.Voice;

public sealed class WakeWordService
{
    public Task<bool> WaitForWakeWordAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(true);
    }
}
