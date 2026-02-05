using System;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.Mono;
using HarmonyLib;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Accessibility.BE6;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger = null!;
    private GameObject? _introSkipper;
    private GameObject? _settingsInjector;
    private GameObject? _autoCollector;
    private Harmony? _autoSkipDialogueHarmony;
    
    // private readonly ConfigEntry<Key> _reloadConfigShortcut;
    internal static ConfigEntry<bool> AutoSkipIntro = null!;
    internal static ConfigEntry<bool> AutoSkipDialogue = null!;
    internal static ConfigEntry<bool> AutoPickupCollectibles = null!;

    public Plugin()
    {
        Logger = base.Logger;
        
        Logger.LogDebug($"Plugin, setting up configs");

        Config.SaveOnConfigSet = true;
        AutoSkipIntro = Config.Bind("Accessibility", "AutoSkipIntro", false,
            "When true, automatically skips the intro cutscene 1 second after the scene fade in begins");
        AutoSkipDialogue = Config.Bind("Accessibility", "AutoSkipDialogue", false,
            "When true, automatically fast forwards all dialogue as fast as the game allows. Does not skip dialogue choices.");
        AutoPickupCollectibles = Config.Bind("Accessibility", "AutoPickupCollectibles", false,
            "When true, automatically picks up collectibles when you aim at them");

        Logger.LogDebug($"HandleAutoSkipDialogue");
        HandleAutoSkipDialogue(AutoSkipDialogue.Value);
        
        // AutoSkipDialogue.SettingChanged += AutoSkipDialogueSettingChanged;
        
        // InputSystem.onEvent += OnInputSystemEvent;
    }

    private void HandleAutoSkipDialogue(bool skipDialogue)
    {
        if (skipDialogue)
        {
            Logger.LogDebug($"Patching in Dialogue Skipper");
            _autoSkipDialogueHarmony ??= Harmony.CreateAndPatchAll(typeof(DialogueSkipperPatch));
        }
        else
        {
            Logger.LogDebug($"Un-patching Dialogue Skipper");
            _autoSkipDialogueHarmony?.UnpatchSelf();
            _autoSkipDialogueHarmony = null;
        }
    }

    // private void AutoSkipDialogueSettingChanged(object sender, EventArgs e)
    // {
    //     var args = (SettingChangedEventArgs)e;
    //     var setting = (ConfigEntry<bool>)args.ChangedSetting;
    //
    //     HandleAutoSkipDialogue(setting.Value);
    // }

    // private void OnInputSystemEvent(InputEventPtr eventPtr, InputDevice device)
    // {
    //     if (!eventPtr.IsA<StateEvent>() && !eventPtr.IsA<DeltaStateEvent>())
    //         return;
    //
    //     if (device is not Keyboard keyboard)
    //         return;
    //
    //     if (keyboard[_reloadConfigShortcut.Value].wasPressedThisFrame)
    //     {
    //         Config.Reload();
    //         Logger.LogInfo($"Config reloaded");
    //     }
    // }

    private void Awake()
    {
        // TODO: change FOV

        Logger.LogDebug($"Creating Settings Injector");
        _settingsInjector = new GameObject();
        _settingsInjector.AddComponent<SettingsInjector>();
        _settingsInjector.transform.parent = gameObject.transform;

        _autoCollector = new GameObject();
        _autoCollector.AddComponent<AutoCollector>();
        _autoCollector.transform.parent = gameObject.transform;
        
        if (AutoSkipIntro.Value)
        {
            Logger.LogDebug($"Creating Intro Skipper");
            _introSkipper = new GameObject();
            _introSkipper.AddComponent<IntroSkipper>();
            _introSkipper.transform.parent = gameObject.transform;
        }

        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
    }

    private void Update()
    {
        // if (UnityEngine.Input.GetKeyDown(_reloadConfigShortcut.Value))
        // {
        //     Config.Reload();
        // }
    }

    private void OnDestroy()
    {
        // InputSystem.onEvent -= OnInputSystemEvent;
        _autoSkipDialogueHarmony?.UnpatchSelf();
        Destroy(_introSkipper);
        Destroy(_settingsInjector);
        Destroy(_autoCollector);
    }
}
