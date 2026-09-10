using System;
using System.Reflection;
using HarmonyLib;

namespace Jcruse03.PortalPass;

[HarmonyPatch(typeof(FejdStartup), nameof(FejdStartup.SetServerToJoin))]
internal static class ServerSelectionPatch
{
    private static void Prefix(ServerJoinData serverData)
    {
        if (serverData.m_type == ServerJoinDataType.Dedicated)
        {
            var dedicated = serverData.Dedicated;
            ConnectionEndpoint.Capture(dedicated.GetHost(), dedicated.m_port);
            return;
        }

        ConnectionEndpoint.Clear();
    }
}

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
        {
            ConnectionEndpoint.Clear();
            return;
        }

        var liveHost = ServerHostField.GetValue(null) as string;
        var portValue = ServerPortField.GetValue(null);
        var livePort = portValue is int value ? value : 0;
        if (!ConnectionEndpoint.TryConsume(liveHost, livePort, out var host, out var port))
        {
            PortalPassPlugin.Log.LogWarning("Could not determine the requested dedicated endpoint after backend resolution. Leaving the vanilla password prompt active.");
            return;
        }

        if (!SecretFile.TryResolve(PortalPassPlugin.SecretFilePath.Value, host, port, PortalPassPlugin.Log, out var password))
        {
            PortalPassPlugin.Log.LogInfo($"No configured password matched {DisplayEndpoint(host, port)}. Leaving the vanilla password prompt active.");
            return;
        }

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
