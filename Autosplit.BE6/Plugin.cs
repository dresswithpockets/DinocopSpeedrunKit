using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.Mono;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Autosplit;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInProcess("Dinocop.exe")]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger = null!;
    private static LiveSplitClient? _client;
    private static Harmony? _harmony;

    private static ConfigEntry<string> _liveSplitAddress = null!;
    private static ConfigEntry<int> _liveSplitPort = null!;
    // private static ConfigEntry<bool> _initGameTime = null!;

    private static ConfigEntry<HashSet<string>> _resetOnScene = null!;
    private static ConfigEntry<SplitConfig> _splits = null!;
    
    private static uint _splitIndex = 0;

    private static bool IsLive => _splitIndex > 0 && _splitIndex < _splits.Value.Splits.Length;

    public Plugin()
    {
        Logger = base.Logger;
        
        TomlTypeConverter.AddConverter(typeof(SplitConfig), new TypeConverter
        {
            ConvertToObject = (input, type) =>
            {
                Debug.Assert(type == typeof(HashSet<string>));

                return input.Split(";").ToHashSet();
            },
            ConvertToString = (input, type) =>
            {
                Debug.Assert(type == typeof(HashSet<string>));
                Debug.Assert(input is HashSet<string>);

                return string.Join("; ", (HashSet<string>)input);
            },
        });
        
        SplitConfig.AddConverters();
        
        _liveSplitAddress = Config.Bind("LiveSplit", "Address", "localhost", "The LiveSplit server address to use.");
        _liveSplitPort = Config.Bind("LiveSplit", "Port", 16834, "The LiveSplit Server port to use.");
        // _initGameTime = Config.Bind("LiveSplit", "UseGameTime", true, "Whether to send game time info to LiveSplit.");

        _resetOnScene = Config.Bind<HashSet<string>>("Splits", "ResetOnScene", ["01_Title_level"],
            "Semicolon-separated list of scene to reset the timer on. This will send a timer reset to LiveSplit whenever ANY of the listed scene are (re)loaded. Some scene are loaded simultaneously, and may remain loaded for the runtime of the game.");
        _splits = Config.Bind("Splits", "Splits", new SplitConfig([]), "Configure which events will trigger LiveSplit splits");

        Logger.LogDebug($"Loaded SplitsConfig: {string.Join(",", _splits.Value.Splits.Select(x => x.ToString()))}");
    }

    private void Awake()
    {
        _client = new LiveSplitClient(_liveSplitAddress.Value, _liveSplitPort.Value);
        try
        {
            Logger.LogInfo("LiveSplit Server Connecting...");
            _client.Connect();
            _client.InitGameTime();
            Logger.LogInfo("LiveSplit Server Connected");
        }
        catch (Exception exception)
        {
            Logger.LogError(exception);
            Logger.LogInfo("Could not connect to LiveSplit server.");
        }
        
        try
        {
            Logger.LogInfo("Applying Harmony Patches");
            _harmony = Harmony.CreateAndPatchAll(typeof(Patches));
        }
        catch (Exception exception)
        {
            Logger.LogError(exception);
            Logger.LogInfo("Could not apply Harmony patches.");
        }
        
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");

        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private void Update()
    {
        if (IsLive && _resetOnScene.Value.Contains(SceneManager.GetActiveScene().name))
        {
            Logger.LogInfo($"Reset Scene was triggered, resetting");
            _client?.Reset();
            _splitIndex = 0;
        }

        // TODO: try handling RTA/IGT.
    }

    private void OnDestroy()
    {
        _client?.Dispose();
        _harmony?.UnpatchSelf();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Logger.LogDebug($"OnSceneLoaded: (name: {scene.name}, mode: {mode})");
        HandleLevelSplit(scene);
    }

    private void OnSceneUnloaded(Scene scene)
    {
        Logger.LogDebug($"OnSceneUnloaded: (name: {scene.name})");
    }

    private static bool TryCurrentSplit(SplitKind kind, [NotNullWhen(true)] out Split? split)
    {
        split = null;
        if (_splitIndex >= _splits.Value.Splits.Length)
        {
            Logger.LogDebug($"Couldn't get current split: the split index was out of bounds");
            return false;
        }

        split = _splits.Value.Splits[_splitIndex];
        if (split.Kind != kind)
        {
            Logger.LogDebug($"Couldn't get current split: the SplitKind didn't match (tried {kind}, configured {split.Kind})");
            return false;
        }

        return true;
    }

    private static void Split()
    {
        Logger.LogInfo("Splitting!");
        _client?.StartOrSplit();
        _splitIndex += 1;
    }

    public static void HandleLevelSplit(Scene scene)
    {
        Logger.LogDebug($"HandleLevelSplit: {scene.name}");
        if (!TryCurrentSplit(SplitKind.Level, out var split))
            return;

        if (split.Value != scene.name)
            return;

        Split();
    }

    public static void HandleScentSplit(Collectible collectible)
    {
        Logger.LogDebug($"HandleScentSplit: {collectible.name}");
        if (!TryCurrentSplit(SplitKind.Scent, out var split))
            return;

        if (split.Value != collectible.type)
            return;

        Split();
    }

    public static void HandleEventSplit(EventInstance @event)
    {
        Logger.LogDebug($"HandleEventSplit: {@event.name}");
        if (!TryCurrentSplit(SplitKind.Event, out var split))
            return;

        if (split.Value != @event.name)
            return;

        Split();
    }

    public static void HandlePermanentSave(Save save)
    {
        Logger.LogDebug($"HandlePermanentSave: {save.savedKey}");
        if (!TryCurrentSplit(SplitKind.PermanentSave, out var split))
            return;

        if (split.Value != save.savedKey)
            return;

        Split();
    }

    public static void HandleUniqueObjectSplit(ObjetUnique objetUnique)
    {
        Logger.LogDebug($"HandleUniqueObjectSplit: {objetUnique.name}");
        if (!TryCurrentSplit(SplitKind.UniqueObject, out var split))
            return;

        if (split.Value != objetUnique.name)
            return;

        Split();
    }

    public static void HandleCollectibleSplit(Collectible collectible)
    {
        Logger.LogDebug($"HandleCollectibleSplit: {collectible.name}");
        if (!TryCurrentSplit(SplitKind.Collectible, out var split))
            return;

        if (split.Value != collectible.name)
            return;

        Split();
    }

    public static void HandleDialogueSplit(Dialogue dialogue)
    {
        Logger.LogDebug($"HandleDialogueSplit: {dialogue.name}");
        if (!TryCurrentSplit(SplitKind.Dialogue, out var split))
            return;

        var name = dialogue.name.Trim().Replace(" (Dialogue)", "");        
        Logger.LogDebug($"                     trimmed name: {name}");
        if (split.Value != name)
            return;

        Split();
    }

    public static void HandleEquipmentSplit(Equipement equipment)
    {
        Logger.LogDebug($"HandleEquipmentSplit: {equipment.name}");
        if (!TryCurrentSplit(SplitKind.Outfit, out var split))
            return;

        if (split.Value != equipment.name)
            return;

        Split();
    }
}
