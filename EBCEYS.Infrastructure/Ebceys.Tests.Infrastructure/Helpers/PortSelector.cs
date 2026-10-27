using System.Net.NetworkInformation;
using JetBrains.Annotations;

namespace Ebceys.Tests.Infrastructure.Helpers;

/// <summary>
///     Utility class for finding available TCP ports for use in integration tests.
/// </summary>
[PublicAPI]
public static class PortSelector
{
    /// <summary>
    ///     Gets the available port.
    /// </summary>
    /// <param name="port">The start port.</param>
    /// <returns>The new available port.</returns>
    public static int GetPort(int port = 0)
    {
        port = port > 0 ? port : new Random().Next(1, 65535);
        while (!IsFree(port))
        {
            port += 1;
        }

        return port;
    }

    /// <summary>
    ///     Checks the <paramref name="port" /> is available.
    /// </summary>
    /// <param name="port">The port number.</param>
    /// <returns>true if port is available; otherwise false.</returns>
    public static bool IsFree(int port)
    {
        var properties = IPGlobalProperties.GetIPGlobalProperties();
        var listeners = properties.GetActiveTcpListeners();
        var openPorts = listeners.Select(item => item.Port).ToArray();
        return openPorts.All(openPort => openPort != port);
    }
}