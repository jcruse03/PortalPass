namespace Jcruse03.PortalPass;

internal static class ConnectionEndpoint
{
    private static string? _pendingHost;
    private static int _pendingPort;

    internal static void Capture(string? host, int port)
    {
        if (string.IsNullOrWhiteSpace(host) || port < 1 || port > 65535)
        {
            Clear();
            return;
        }

        _pendingHost = host;
        _pendingPort = port;
    }

    internal static void Clear()
    {
        _pendingHost = null;
        _pendingPort = 0;
    }

    internal static bool TryConsume(string? liveHost, int livePort, out string host, out int port)
    {
        if (!string.IsNullOrWhiteSpace(_pendingHost) && _pendingPort is >= 1 and <= 65535)
        {
            host = _pendingHost!;
            port = _pendingPort;
            Clear();
            return true;
        }

        Clear();
        if (!string.IsNullOrWhiteSpace(liveHost) && livePort is >= 1 and <= 65535)
        {
            host = liveHost!;
            port = livePort;
            return true;
        }

        host = string.Empty;
        port = 0;
        return false;
    }
}
