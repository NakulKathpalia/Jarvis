namespace Jarvis.Core.Agents.Coding.Runnable;

using System.Net;
using System.Net.Sockets;

/// <summary>
/// Finds available local ports.
/// </summary>
public sealed class PortFinder
{
    /// <summary>
    /// Finds an available port starting at the specified port.
    /// </summary>
    public int FindAvailablePort(int startPort)
    {
        for (var port = Math.Max(1, startPort); port <= 65535; port++)
        {
            if (IsAvailable(port))
            {
                return port;
            }
        }

        throw new InvalidOperationException("No available local port was found.");
    }

    private static bool IsAvailable(int port)
    {
        try
        {
            using var listener = new TcpListener(IPAddress.Loopback, port);
            listener.Start();
            return true;
        }
        catch (SocketException)
        {
            return false;
        }
    }
}
