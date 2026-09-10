using System;
using System.Reflection;
using HarmonyLib;

namespace Jcruse03.PortalPass;

[HarmonyPatch(typeof(ZNet), "RPC_ClientHandshake")]
internal static class PasswordHandshakePatch
{
    private static readonly FieldInfo ServerHostField = AccessTools.Field(typeof(ZNet), "m_serverHost");
    private static readonly FieldInfo ServerPortField = AccessTools.Field(typeof(ZNet), "m_serverHostPort");
    private static readonly FieldInfo ServerPasswordField = AccessTools.Field(typeof(FejdStartup), "<ServerPassword>k__BackingField");
    private static string? _injectedPassword;

    private static void Prefix(bool needPassword)
    {
        ClearOwnedPassword();
        if (!needPassword)
            return;

        var host = ServerHostField.GetValue(null) as string;
        var portValue = ServerPortField.GetValue(null);
        if (host == null || string.IsNullOrWhiteSpace(host) || !(portValue is int port))
            return;

        if (!SecretFile.TryResolve(PortalPassPlugin.SecretFilePath.Value, host, port, PortalPassPlugin.Log, out var password))
            return;

        _injectedPassword = password;
        ServerPasswordField.SetValue(null, password);
        PortalPassPlugin.Log.LogInfo($"Supplying a configured password for {DisplayEndpoint(host, port)}.");
    }

    private static void Postfix() => ClearOwnedPassword();

    private static void ClearOwnedPassword()
    {
        if (_injectedPassword != null && string.Equals(FejdStartup.ServerPassword, _injectedPassword, StringComparison.Ordinal))
            ServerPasswordField.SetValue(null, null);
        _injectedPassword = null;
    }

    private static string DisplayEndpoint(string host, int port) =>
        host.Contains(":") ? $"[{host}]:{port}" : $"{host}:{port}";
}
