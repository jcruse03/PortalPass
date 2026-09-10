using System;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

namespace Jcruse03.PortalPass;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class PortalPassPlugin : BaseUnityPlugin
{
    internal const string PluginGuid = "com.jcruse03.portalpass";
    internal const string PluginName = "PortalPass";
    internal const string PluginVersion = "0.1.1";

    internal static ConfigEntry<string> SecretFilePath = null!;
    internal static ManualLogSource Log = null!;

    private Harmony? _harmony;

    private void Awake()
    {
        Log = Logger;
        SecretFilePath = Config.Bind(
            "General",
            "SecretFilePath",
            SecretFile.DefaultPath(),
            "Absolute path (or ~/ path) to the endpoint=password secret file. Keep this outside r2modman profiles.");

        _harmony = new Harmony(PluginGuid);
        _harmony.PatchAll(Assembly.GetExecutingAssembly());
        Logger.LogInfo($"{PluginName} {PluginVersion} loaded (client-only). Secrets: {SecretFile.ExpandPath(SecretFilePath.Value)}");
    }

    private void OnDestroy() => _harmony?.UnpatchSelf();
}
