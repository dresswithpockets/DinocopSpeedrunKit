using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Autosplit.BE5;

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
    private static ConfigEntry<bool> _initGameTime = null!;

    private static ConfigEntry<string> _resetOnScene = null!;
    private static ConfigEntry<SplitConfig> _splits = null!;
    
    private static uint _splitIndex = 0;

    public Plugin()
    {
        Logger = base.Logger;
        
        SplitConfig.AddConverters();
        
        _liveSplitAddress = Config.Bind("LiveSplit", "Address", "localhost", "The LiveSplit server address to use.");
        _liveSplitPort = Config.Bind("LiveSplit", "Port", 16834, "The LiveSplit Server port to use.");
        _initGameTime = Config.Bind("LiveSplit", "UseGameTime", true,
            "Whether or not to send game time info to LiveSplit.");

        _resetOnScene = Config.Bind("Splits", "ResetOnScene", "01_Title_level",
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
            _harmony = Harmony.CreateAndPatchAll(typeof(Plugin));
        }
        catch (Exception exception)
        {
            Logger.LogError(exception);
            Logger.LogInfo("Could not apply Harmony patches.");
        }
        
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Update()
    {
        if (_splitIndex > 0 && _resetOnScene.Value.Split(';').Contains(SceneManager.GetActiveScene().name))
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
        Logger.LogInfo($"HandleLevelSplit: {scene.name}");
        if (!TryCurrentSplit(SplitKind.Level, out var split))
            return;

        if (split.Value != scene.name)
            return;

        Split();
    }

    public static void HandleScentSplit(Collectible collectible)
    {
        Logger.LogInfo($"HandleScentSplit: {collectible.name}");
        if (!TryCurrentSplit(SplitKind.Scent, out var split))
            return;

        if (split.Value != collectible.type)
            return;

        Split();
    }

    public static void HandleEventSplit(EventInstance @event)
    {
        Logger.LogInfo($"HandleEventSplit: {@event.name}");
        if (!TryCurrentSplit(SplitKind.Event, out var split))
            return;

        if (split.Value != @event.name)
            return;

        Split();
    }

    public static void HandleCollectibleSplit(Collectible collectible)
    {
        Logger.LogInfo($"HandleCollectibleSplit: {collectible.name}");
        if (!TryCurrentSplit(SplitKind.Collectible, out var split))
            return;

        if (split.Value != collectible.name)
            return;

        Split();
    }

    public static void HandleDialogueSplit(Dialogue dialogue)
    {
        Logger.LogInfo($"HandleDialogueSplit: {dialogue.name}");
        if (!TryCurrentSplit(SplitKind.Dialogue, out var split))
            return;

        var name = dialogue.name.Trim().Replace(" (Dialogue)", "");        
        Logger.LogInfo($"                     trimmed name: {name}");
        if (split.Value != name)
            return;

        Split();
    }

    private void UpdateTimer()
    {
        // throw new NotImplementedException();        
    }

    #region Patches

    
    [HarmonyPatch(typeof(Inventaire), "AddCollectible")]
    [HarmonyPrefix]
    private static bool PreAddCollectible(Collectible _collectible, float _value = 0.0f)
    {
        Logger.LogInfo($"PreAddCollectible: (name: {_collectible.name})");
        HandleCollectibleSplit(_collectible);
        return true;
    }
    
    [HarmonyPatch(typeof(Inventaire), "CheckLearnScent")]
    [HarmonyPrefix]
    private static bool PreCheckLearnScent(Collectible _collectible, Inventaire __instance)
    {
        Logger.LogInfo($"PreCheckLearnScent: (name: {_collectible.type})");
        if (_collectible.cantLearn || __instance.scentsLearned == null ||
            __instance.scentsLearned.Contains(_collectible.type))
            return true;
        
        HandleScentSplit(_collectible);
        return true;
    }
    
    [HarmonyPatch(typeof(Exit), "TriggerExit")]
    [HarmonyPrefix]
    private static bool PreTriggerExit(Exit __instance)
    {
        Logger.LogInfo($"PreTriggerExit: (exitingLevel: {LevelManager.instance.exitingLevel}, horraire: {__instance.horraire}, CestOuvert(): {__instance.CestOuvert()}, localMode: {__instance.localMode}, globalID: {__instance.globalID}, localID: {__instance.localID}, destinationID: {__instance.destinationID}, goToThisLevel: {__instance.goToThisLevel})");
        return true;
    }

    [HarmonyPatch(typeof(Exit), "TestCollision")]
    [HarmonyPrefix]
    private static bool PreTestCollisionExit(GameObject other, Exit __instance, float ____safetyDelay)
    {
        Logger.LogInfo($"PreTestCollisionExit: (exitingLevel: {LevelManager.instance.exitingLevel}, horraire: {__instance.horraire}, CestOuvert(): {__instance.CestOuvert()}, localMode: {__instance.localMode}, globalID: {__instance.globalID}, localID: {__instance.localID}, destinationID: {__instance.destinationID}, goToThisLevel: {__instance.goToThisLevel}, collisionMode: {__instance.collisionMode}, canExit: {LevelManager.instance.canExit}, tag: {other.tag})");
        return true;
    }
    
    [HarmonyPatch(typeof(DialogueManager), "StartDialogue")]
    [HarmonyPrefix]
    private static bool PreStartDialogueDialogueManager(Dialogue _dialogue)
    {
        Logger.LogInfo($"PreStartDialogueDialogueManager: (name: {_dialogue})");
        HandleDialogueSplit(_dialogue);
        return true;
    }

    [HarmonyPatch(typeof(EventInstance), "HandleEvents")]
    [HarmonyPrefix]
    private static bool PreHandleEvents(float _eventDelay, EventInstance __instance)
    {
        Debug.Log("BLAH BLAH BLAH BLAH BLAH UNITYENGINE DEBUG");
        Console.WriteLine("BLAH BLAH BLAH BLAH BLAH BLAH BLAH");
        var events = string.Join(", ", __instance.events);
        Logger.LogInfo($"PreHandleEvents: (name: {__instance.name}, delay: {_eventDelay:.4f}, activeInHierarchy: {__instance.gameObject.activeInHierarchy}, events: [{events}])");
        HandleEventSplit(__instance);
        return true;
    }
    
    [HarmonyPatch(typeof(EconomyManager), "GiveEquipementToPlayer")]
    [HarmonyPrefix]
    private static bool PreGiveEquipment(Equipement _equip, bool _big = false, bool _small = false, bool _faster = false)
    {
        Logger.LogInfo($"PreGiveEquipment: (name: {_equip})");
        return true;
    }

    [HarmonyPatch(typeof(Inventaire), "AddCollectible")]
    [HarmonyPrefix]
    private static bool PreGiveCollectible(Collectible _collectible, float _value = 0.0f)
    {
        var name = GetScriptableObjectName(_collectible);
        Logger.LogInfo($"PreGiveCollectible: (name: {name})");
        return true;
    }
    
    [HarmonyPatch(typeof(EconomyManager), "GiveScentToPlayer")]
    [HarmonyPrefix]
    private static bool PreGiveScent(ScentAsset _scent)
    {
        var name = GetScriptableObjectName(_scent);
        Logger.LogInfo($"PreGiveScent: (name: {name})");
        return true;
    }
    
    private static string GetScriptableObjectName<T>(T item) where T : ScriptableObject
    {
        var name = item.ToString();
        var postfixLength = typeof(T).Name.Length + 3;
        return name.Substring(0, name.Length - postfixLength);
    }

    #endregion
}
