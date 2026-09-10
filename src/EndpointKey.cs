using System;
using System.Globalization;
using System.Net;

namespace Jcruse03.PortalPass;

internal readonly struct EndpointKey : IEquatable<EndpointKey>
{
    internal EndpointKey(string host, int? port)
    {
        Host = host;
        Port = port;
    }

    internal string Host { get; }
    internal int? Port { get; }

    internal static EndpointKey Parse(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new FormatException("Endpoint is empty.");

        var value = text.Trim();
        string host;
        int? port = null;

        if (value[0] == '[')
        {
            var close = value.IndexOf(']');
            if (close < 0)
                throw new FormatException("Bracketed IPv6 endpoint is missing ']'.");

            host = value.Substring(1, close - 1);
            var remainder = value.Substring(close + 1);
            if (remainder.Length > 0)
            {
                if (remainder[0] != ':')
                    throw new FormatException("Unexpected text after bracketed host.");
                port = ParsePort(remainder.Substring(1));
            }
        }
        else
        {
            var firstColon = value.IndexOf(':');
            var lastColon = value.LastIndexOf(':');
            if (firstColon >= 0 && firstColon == lastColon)
            {
                host = value.Substring(0, firstColon);
                port = ParsePort(value.Substring(firstColon + 1));
            }
            else
            {
                host = value;
            }
        }

        return new EndpointKey(NormalizeHost(host), port);
    }

    internal static EndpointKey FromConnection(string host, int port)
    {
        if (port < 1 || port > 65535)
            throw new FormatException("Connection port is outside 1-65535.");
        return new EndpointKey(NormalizeHost(host), port);
    }

    internal EndpointKey WithoutPort() => new(Host, null);

    private static int ParsePort(string value)
    {
        if (!int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var port) || port < 1 || port > 65535)
            throw new FormatException("Port must be an integer from 1 through 65535.");
        return port;
    }

    private static string NormalizeHost(string value)
    {
        var host = value.Trim();
        if (host.Length == 0)
            throw new FormatException("Host is empty.");
        if (host.IndexOfAny(new[] { '/', '\\', '@', '#', '=', ' ', '\t', '\r', '\n' }) >= 0)
            throw new FormatException("Host contains unsupported characters.");

        if (IPAddress.TryParse(host, out var address))
            return address.ToString().ToLowerInvariant();

        host = host.TrimEnd('.').ToLowerInvariant();
        if (host.Length == 0)
            throw new FormatException("Host is empty after normalization.");
        return host;
    }

    public bool Equals(EndpointKey other) =>
        Port == other.Port && string.Equals(Host, other.Host, StringComparison.Ordinal);

    public override bool Equals(object? obj) => obj is EndpointKey other && Equals(other);
    public override int GetHashCode() => (Host.GetHashCode() * 397) ^ (Port ?? 0);
    public override string ToString() => Port.HasValue
        ? (Host.Contains(":") ? $"[{Host}]:{Port.Value}" : $"{Host}:{Port.Value}")
        : Host;
}
