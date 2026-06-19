using Jarvis.Models;

namespace Jarvis.Services;

public interface IPcControlService
{
    Task<string> ExecuteAsync(PcCommand command, CancellationToken cancellationToken = default);
}
