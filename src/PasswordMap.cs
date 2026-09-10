using System;
using System.Collections.Generic;
using System.IO;

namespace Jcruse03.PortalPass;

internal sealed class PasswordMap
{
    private readonly Dictionary<EndpointKey, string> _passwords;

    private PasswordMap(Dictionary<EndpointKey, string> passwords) => _passwords = passwords;

    internal static PasswordMap Parse(IEnumerable<string> lines)
    {
        var passwords = new Dictionary<EndpointKey, string>();
        var lineNumber = 0;

        foreach (var rawLine in lines)
        {
            lineNumber++;
            var line = rawLine.Trim();
            if (line.Length == 0 || line.StartsWith("#", StringComparison.Ordinal))
                continue;

            var separator = line.IndexOf('=');
            if (separator < 1)
                throw new FormatException($"Line {lineNumber}: expected endpoint=password.");

            EndpointKey endpoint;
            try
            {
                endpoint = EndpointKey.Parse(line.Substring(0, separator));
            }
            catch (FormatException exception)
            {
                throw new FormatException($"Line {lineNumber}: {exception.Message}", exception);
            }

            var password = Unquote(line.Substring(separator + 1).Trim());
            if (password.Length == 0)
                throw new FormatException($"Line {lineNumber}: password is empty.");
            if (passwords.ContainsKey(endpoint))
                throw new FormatException($"Line {lineNumber}: duplicate endpoint after normalization: {endpoint}.");
            passwords.Add(endpoint, password);
        }

        return new PasswordMap(passwords);
    }

    internal static PasswordMap Load(string path) => Parse(File.ReadLines(path));

    internal bool TryResolve(string host, int port, out string password)
    {
        var exact = EndpointKey.FromConnection(host, port);
        return _passwords.TryGetValue(exact, out password!) ||
               _passwords.TryGetValue(exact.WithoutPort(), out password!);
    }

    private static string Unquote(string value)
    {
        if (value.Length < 2)
            return value;

        var first = value[0];
        var last = value[value.Length - 1];
        if ((first == '\'' && last == '\'') || (first == '"' && last == '"'))
            return value.Substring(1, value.Length - 2);
        if (first == '\'' || first == '"' || last == '\'' || last == '"')
            throw new FormatException("Password has an unmatched quote.");
        return value;
    }
}
