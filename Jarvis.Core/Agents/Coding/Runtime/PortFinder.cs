namespace Jarvis.Core.Agents.Coding.Runtime;

using System.Net;
using System.Net.Sockets;

/// <summary>
/// Finds available ports for local development servers.
/// </summary>
public sealed class PortFinder
{
    /// <summary>
    /// Finds the first available port at or after the start port.
    /// </summary>
    public int Find(int startPort = 5173)
    {
        for (var port = Math.Max(1, startPort); port <= 65535; port++)
        {
            if (Available(port))
            {
                return port;
            }
        }

        throw new InvalidOperationException("No available port was found.");
    }

    private static bool Available(int port)
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
