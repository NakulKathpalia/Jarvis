namespace Jarvis.Services;

public sealed class PlatformService
{
    public string Current
    {
        get
        {
            if (OperatingSystem.IsWindows())
            {
                return "Windows";
            }

            if (OperatingSystem.IsMacOS())
            {
                return "macOS";
            }

            if (OperatingSystem.IsLinux())
            {
                return "Linux";
            }

            return "Unknown";
        }
    }

    public bool IsWindows => OperatingSystem.IsWindows();
    public bool IsLinux => OperatingSystem.IsLinux();
    public bool IsMacOS => OperatingSystem.IsMacOS();
}
