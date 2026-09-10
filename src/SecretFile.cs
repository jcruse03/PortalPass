using System;
using System.IO;
using BepInEx.Logging;

namespace Jcruse03.PortalPass;

internal static class SecretFile
{
    internal static string DefaultPath()
    {
        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PortalPass", "passwords.env");

        var configHome = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME");
        if (string.IsNullOrWhiteSpace(configHome))
            configHome = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config");
        return Path.Combine(configHome, "portalpass", "passwords.env");
    }

    internal static string ExpandPath(string path)
    {
        var expanded = Environment.ExpandEnvironmentVariables(path.Trim());
        if (expanded == "~")
            return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        if (expanded.StartsWith("~/", StringComparison.Ordinal) || expanded.StartsWith("~\\", StringComparison.Ordinal))
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), expanded.Substring(2));
        return Path.GetFullPath(expanded);
    }

    internal static bool TryResolve(string path, string host, int port, ManualLogSource logger, out string password)
    {
        password = string.Empty;
        try
        {
            var expanded = ExpandPath(path);
            if (!File.Exists(expanded))
            {
                logger.LogWarning($"Secret file not found: {expanded}. Leaving the vanilla password prompt active.");
                return false;
            }

            return PasswordMap.Load(expanded).TryResolve(host, port, out password);
        }
        catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException || exception is FormatException || exception is ArgumentException)
        {
            logger.LogWarning($"Could not load PortalPass secrets ({exception.Message}). Leaving the vanilla password prompt active.");
            return false;
        }
    }
}
