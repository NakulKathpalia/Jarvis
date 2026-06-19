using System.Diagnostics;

namespace Jarvis.Services;

public sealed class InstalledAppService
{
    public Task<bool> OpenAsync(string target)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = target,
                UseShellExecute = true
            });
            return Task.FromResult(true);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }
}
