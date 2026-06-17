using System.Diagnostics;
using System.Runtime.InteropServices;
using Jarvis.Models;

namespace Jarvis.Services;

public sealed class WindowsPcControlService : IPcControlService
{
    private const byte VolumeMuteKey = 0xAD;
    private const byte VolumeDownKey = 0xAE;
    private const byte VolumeUpKey = 0xAF;
    private const uint KeyEventExtendedKey = 0x0001;
    private const uint KeyEventKeyUp = 0x0002;

    private readonly string _screenshotDirectory;

    public WindowsPcControlService(string screenshotDirectory)
    {
        _screenshotDirectory = screenshotDirectory;
        Directory.CreateDirectory(_screenshotDirectory);
    }

    public Task<string> ExecuteAsync(PcCommand command, CancellationToken cancellationToken = default)
    {
        var message = command.Action switch
        {
            PcControlAction.OpenApp => OpenShellTarget(command.Target, "app"),
            PcControlAction.OpenWebsite => OpenShellTarget(NormalizeWebsite(command.Target), "website"),
            PcControlAction.OpenFolder => OpenFolder(command.Target),
            PcControlAction.OpenFile => OpenFile(command.Target),
            PcControlAction.BrowserSearch => OpenShellTarget(BuildSearchUrl(command.Target), "browser search"),
            PcControlAction.TakeScreenshot => TakeScreenshot(),
            PcControlAction.VolumeUp => SendVolumeKey(VolumeUpKey, "Volume up."),
            PcControlAction.VolumeDown => SendVolumeKey(VolumeDownKey, "Volume down."),
            PcControlAction.ToggleMute => SendVolumeKey(VolumeMuteKey, "Volume mute toggled."),
            PcControlAction.Sleep => RunControlledProcess("rundll32.exe", "powrprof.dll,SetSuspendState 0,1,0", "Sleep requested."),
            PcControlAction.Shutdown => RunControlledProcess("shutdown.exe", "/s /t 30", "Shutdown scheduled in 30 seconds."),
            PcControlAction.Restart => RunControlledProcess("shutdown.exe", "/r /t 30", "Restart scheduled in 30 seconds."),
            _ => "Unsupported PC control command."
        };

        return Task.FromResult(message);
    }

    private static string OpenShellTarget(string target, string label)
    {
        if (string.IsNullOrWhiteSpace(target))
        {
            return $"No {label} target provided.";
        }

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = target,
                UseShellExecute = true
            });
            return $"Opened {label}: {target}";
        }
        catch (Exception ex)
        {
            return $"Could not open {label}: {ex.Message}";
        }
    }

    private static string OpenFolder(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
        {
            return "Folder was not found.";
        }

        return OpenShellTarget(path, "folder");
    }

    private static string OpenFile(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            return "File was not found.";
        }

        return OpenShellTarget(path, "file");
    }

    private string TakeScreenshot()
    {
        try
        {
            var screenBounds = GetVirtualScreenBounds();
            var bytes = CaptureScreenBmp(screenBounds.Left, screenBounds.Top, screenBounds.Width, screenBounds.Height);
            var fileName = $"screenshot-{DateTime.Now:yyyyMMdd-HHmmss}.bmp";
            var path = Path.Combine(_screenshotDirectory, fileName);
            File.WriteAllBytes(path, bytes);
            return $"Screenshot saved: {path}";
        }
        catch (Exception ex)
        {
            return $"Screenshot failed: {ex.Message}";
        }
    }

    private static ScreenBounds GetVirtualScreenBounds()
    {
        var left = GetSystemMetrics(76);
        var top = GetSystemMetrics(77);
        var width = GetSystemMetrics(78);
        var height = GetSystemMetrics(79);
        return new ScreenBounds(left, top, width, height);
    }

    private static byte[] CaptureScreenBmp(int left, int top, int width, int height)
    {
        var screenDc = GetDC(IntPtr.Zero);
        var memoryDc = CreateCompatibleDC(screenDc);
        var bitmap = CreateCompatibleBitmap(screenDc, width, height);
        var previousObject = SelectObject(memoryDc, bitmap);

        try
        {
            if (!BitBlt(memoryDc, 0, 0, width, height, screenDc, left, top, 0x00CC0020))
            {
                throw new InvalidOperationException("Screen copy failed.");
            }

            var header = new BitmapInfoHeader
            {
                Size = Marshal.SizeOf<BitmapInfoHeader>(),
                Width = width,
                Height = height,
                Planes = 1,
                BitCount = 32,
                Compression = 0,
                SizeImage = width * height * 4
            };

            var pixels = new byte[header.SizeImage];
            var bitmapInfo = new BitmapInfo { Header = header };
            var result = GetDIBits(screenDc, bitmap, 0, (uint)height, pixels, ref bitmapInfo, 0);
            if (result == 0)
            {
                throw new InvalidOperationException("Bitmap export failed.");
            }

            return BuildBmpFile(header, pixels);
        }
        finally
        {
            SelectObject(memoryDc, previousObject);
            DeleteObject(bitmap);
            DeleteDC(memoryDc);
            ReleaseDC(IntPtr.Zero, screenDc);
        }
    }

    private static byte[] BuildBmpFile(BitmapInfoHeader header, byte[] pixels)
    {
        const int fileHeaderSize = 14;
        var dibHeaderSize = Marshal.SizeOf<BitmapInfoHeader>();
        var pixelOffset = fileHeaderSize + dibHeaderSize;
        var fileSize = pixelOffset + pixels.Length;

        using var stream = new MemoryStream(fileSize);
        using var writer = new BinaryWriter(stream);

        writer.Write((byte)'B');
        writer.Write((byte)'M');
        writer.Write(fileSize);
        writer.Write((short)0);
        writer.Write((short)0);
        writer.Write(pixelOffset);

        writer.Write(header.Size);
        writer.Write(header.Width);
        writer.Write(header.Height);
        writer.Write(header.Planes);
        writer.Write(header.BitCount);
        writer.Write(header.Compression);
        writer.Write(header.SizeImage);
        writer.Write(header.XPixelsPerMeter);
        writer.Write(header.YPixelsPerMeter);
        writer.Write(header.ColorsUsed);
        writer.Write(header.ColorsImportant);
        writer.Write(pixels);

        return stream.ToArray();
    }

    private static string SendVolumeKey(byte key, string message)
    {
        keybd_event(key, 0, KeyEventExtendedKey, UIntPtr.Zero);
        keybd_event(key, 0, KeyEventExtendedKey | KeyEventKeyUp, UIntPtr.Zero);
        return message;
    }

    private static string RunControlledProcess(string fileName, string arguments, string successMessage)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                UseShellExecute = false,
                CreateNoWindow = true
            });
            return successMessage;
        }
        catch (Exception ex)
        {
            return $"System action failed: {ex.Message}";
        }
    }

    private static string NormalizeWebsite(string target)
    {
        var trimmed = target.Trim();
        if (trimmed.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            || trimmed.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return trimmed;
        }

        if (!trimmed.Contains('.', StringComparison.Ordinal))
        {
            return $"https://www.{trimmed}.com";
        }

        return $"https://{trimmed}";
    }

    private static string BuildSearchUrl(string query)
    {
        return $"https://www.google.com/search?q={Uri.EscapeDataString(query.Trim())}";
    }

    [DllImport("user32.dll")]
    private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

    [DllImport("user32.dll")]
    private static extern int GetSystemMetrics(int nIndex);

    [DllImport("user32.dll")]
    private static extern IntPtr GetDC(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDc);

    [DllImport("gdi32.dll")]
    private static extern IntPtr CreateCompatibleDC(IntPtr hDc);

    [DllImport("gdi32.dll")]
    private static extern IntPtr CreateCompatibleBitmap(IntPtr hDc, int width, int height);

    [DllImport("gdi32.dll")]
    private static extern IntPtr SelectObject(IntPtr hDc, IntPtr hObject);

    [DllImport("gdi32.dll")]
    private static extern bool BitBlt(
        IntPtr destinationDc,
        int x,
        int y,
        int width,
        int height,
        IntPtr sourceDc,
        int sourceX,
        int sourceY,
        int rasterOperation);

    [DllImport("gdi32.dll")]
    private static extern int GetDIBits(
        IntPtr hDc,
        IntPtr hBitmap,
        uint startScan,
        uint scanLines,
        byte[] bits,
        ref BitmapInfo bitmapInfo,
        uint usage);

    [DllImport("gdi32.dll")]
    private static extern bool DeleteObject(IntPtr hObject);

    [DllImport("gdi32.dll")]
    private static extern bool DeleteDC(IntPtr hDc);

    private sealed record ScreenBounds(int Left, int Top, int Width, int Height);

    [StructLayout(LayoutKind.Sequential)]
    private struct BitmapInfo
    {
        public BitmapInfoHeader Header;
        public uint Colors;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct BitmapInfoHeader
    {
        public int Size;
        public int Width;
        public int Height;
        public short Planes;
        public short BitCount;
        public int Compression;
        public int SizeImage;
        public int XPixelsPerMeter;
        public int YPixelsPerMeter;
        public int ColorsUsed;
        public int ColorsImportant;
    }
}
