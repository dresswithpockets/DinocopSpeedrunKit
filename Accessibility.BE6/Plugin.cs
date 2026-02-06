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
    private GameObject? _holdToInteract;
    private Harmony? _autoSkipDialogueHarmony;
    
    internal static ConfigEntry<bool> AutoSkipIntro = null!;
    internal static ConfigEntry<bool> AutoSkipDialogue = null!;
    internal static ConfigEntry<bool> AutoPickupCollectibles = null!;
    internal static ConfigEntry<KeyCode> HoldToInteractKey = null!;

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
        HoldToInteractKey = Config.Bind("Accessibility", "HoldToInteractKey", KeyCode.Mouse2,
            "When held down, will simulate an Interact click every frame");

        AutoSkipIntro.SettingChanged += AutoSkipIntroSettingChanged;
        AutoSkipDialogue.SettingChanged += AutoSkipDialogueSettingChanged;
        AutoPickupCollectibles.SettingChanged += AutoPickupCollectiblesSettingChanged;
    }

    private void Awake()
    {
        // TODO: change FOV

        Logger.LogDebug($"Creating handler objects");
        _settingsInjector = new GameObject();
        _settingsInjector.AddComponent<SettingsInjector>();
        _settingsInjector.transform.parent = gameObject.transform;

        _holdToInteract = new GameObject();
        _holdToInteract.AddComponent<HoldToInteract>();
        _holdToInteract.transform.parent = gameObject.transform;
        
        HandleAutoSkipIntro(AutoSkipIntro.Value);
        HandleAutoSkipDialogue(AutoSkipDialogue.Value);
        HandleAutoPickupCollectibles(AutoPickupCollectibles.Value);

        Logger.LogInfo($"Finished loading!");
    }

    private void OnDestroy()
    {
        _autoSkipDialogueHarmony?.UnpatchSelf();
        Destroy(_holdToInteract);
        Destroy(_introSkipper);
        Destroy(_settingsInjector);
        Destroy(_autoCollector);
    }

    private void HandleAutoSkipIntro(bool skipIntro)
    {
        if (skipIntro)
        {
            if (_introSkipper) return;

            _introSkipper = new GameObject();
            _introSkipper.AddComponent<IntroSkipper>();
            _introSkipper.transform.parent = gameObject.transform;
        }
        else
        {
            Destroy(_introSkipper);
            _introSkipper = null;
        }
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

    private void HandleAutoPickupCollectibles(bool autoPickup)
    {
        if (autoPickup)
        {
            if (_autoCollector) return;

            _autoCollector = new GameObject();
            _autoCollector.AddComponent<AutoCollector>();
            _autoCollector.transform.parent = gameObject.transform;
        }
        else
        {
            Destroy(_autoCollector);
            _autoCollector = null;
        }
    }

    private void AutoSkipIntroSettingChanged(object sender, EventArgs e)
    {
        var args = (SettingChangedEventArgs)e;
        var setting = (ConfigEntry<bool>)args.ChangedSetting;
    
        HandleAutoSkipIntro(setting.Value);
    }

    private void AutoSkipDialogueSettingChanged(object sender, EventArgs e)
    {
        var args = (SettingChangedEventArgs)e;
        var setting = (ConfigEntry<bool>)args.ChangedSetting;
    
        HandleAutoSkipDialogue(setting.Value);
    }

    private void AutoPickupCollectiblesSettingChanged(object sender, EventArgs e)
    {
        var args = (SettingChangedEventArgs)e;
        var setting = (ConfigEntry<bool>)args.ChangedSetting;
    
        HandleAutoPickupCollectibles(setting.Value);
    }
}
