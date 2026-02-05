using System;
using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Accessibility.BE6;


public class SettingsInjector : MonoBehaviour
{
    [HarmonyPatch(typeof(SettingsManager), "OnEnable")]
    [HarmonyPrefix]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    private static void PreSettingsManagerOnEnable(SettingsManager __instance)
    {
        // singleton-like behaviour to avoid calls to FindObjectOfType every frame
        Plugin.Logger.LogDebug($"SettingsInjector: SettingsManager OnEnable");
        _settingsManager = __instance;
    }

    private const string IntroSceneName = "01_Title_level";
    
    private const string AutoSkipIntroDialogueKey = "UI_DCSK_Accessibility_AutoSkipIntro";
    private static readonly DialogueInstance AutoSkipIntroDialogue = new()
    {
        personnage = AutoSkipIntroDialogueKey,
        english = "Automatically skip intro cutscene",
        francais = "Passer automatiquement l'intro", // i dont know french lmfao
    };
    
    private const string AutoSkipDialogueDialogueKey = "UI_DCSK_Accessibility_AutoSkipDialogue";
    private static readonly DialogueInstance AutoSkipDialogueDialogue = new()
    {
        personnage = AutoSkipDialogueDialogueKey,
        english = "Automatically skip dialogue",
        francais = "Passer automatiquement le dialogue", // i dont know french lmfao
    };

    private const string AutoPickupCollectiblesDialogueKey = "UI_DCSK_Accessibility_AutoPickupCollectibles";
    private static readonly DialogueInstance AutoPickupCollectiblesDialogue = new()
    {
        personnage = AutoPickupCollectiblesDialogueKey,
        english = "Aim at collectibles to pick them up",
        francais = "Visez les objets à collectionner pour les ramasser", // i dont know french lmfao
    };

    private static SettingsManager? _settingsManager;
    private Harmony? _harmony;

    public void Awake()
    {
        Plugin.Logger.LogDebug($"SettingsInjector: Awake");
        
        _harmony = Harmony.CreateAndPatchAll(typeof(SettingsInjector));

        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private void OnDestroy()
    {
        Plugin.Logger.LogDebug($"SettingsInjector: OnDestroy");
        
        _harmony?.UnpatchSelf();
        
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    public void Start()
    {
        Plugin.Logger.LogDebug($"SettingsInjector: Disabling on start");
        gameObject.SetActive(false);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Plugin.Logger.LogDebug($"SettingsInjector: OnSceneLoaded");
        if (scene.name == IntroSceneName)
        {
            Plugin.Logger.LogDebug($"SettingsInjector: Enabling to set up custom accessibility options");
            gameObject.SetActive(true);
        }
    }

    private void OnSceneUnloaded(Scene scene)
    {
        Plugin.Logger.LogDebug($"SettingsInjector: OnSceneUnloaded");
        if (scene.name == IntroSceneName)
        {
            Plugin.Logger.LogDebug($"SettingsInjector: Disabling");
            gameObject.SetActive(false);
        }
    }

    private void AddAccessibilityOption(string optionName, UnityAction<bool> onValueChanged, bool defaultValue,
        string keycode)
    {
        var accessibilityOptionPrefab = _settingsManager!.cameraFollowsSlopes.transform.parent.gameObject;

        var option = Instantiate(accessibilityOptionPrefab, accessibilityOptionPrefab.transform.parent);
        option.name = optionName;

        var toggle = option.GetComponentInChildren<Toggle>();
        toggle.onValueChanged.RemoveAllListeners();
        toggle.onValueChanged.AddListener(onValueChanged);
        toggle.isOn = defaultValue;

        var label = option.GetComponentInChildren<TextLocalisation>();
        label.keycode = keycode;
        label.UpdateThisText();
    }

    private void Update()
    {
        if (!TitleScreenManager.instance || !TitleScreenManager.instance.csvReader || !TitleScreenManager.instance.csvReader.inited)
            return;

        TitleScreenManager.instance.csvReader.dialogueDictionary.TryAdd(AutoSkipIntroDialogueKey, AutoSkipIntroDialogue);
        TitleScreenManager.instance.csvReader.dialogueDictionary.TryAdd(AutoSkipDialogueDialogueKey, AutoSkipDialogueDialogue);
        TitleScreenManager.instance.csvReader.dialogueDictionary.TryAdd(AutoPickupCollectiblesDialogueKey, AutoPickupCollectiblesDialogue);
        
        if (!_settingsManager)
            return;
        
        Plugin.Logger.LogDebug($"SettingsInjector: Setting up the new accessibility options...");

        // the cameraFollowsSlopes's parent includes both the toggle object and its label. This is what we want to clone
        // and modify to create our own entries in the menu.
        try
        {
            var delta = _settingsManager!.generalScroll.content.sizeDelta;
            delta.y += 90;
            _settingsManager.generalScroll.content.sizeDelta = delta;

            AddAccessibilityOption(
                "AutoSkipIntroOption",
                value => Plugin.AutoSkipIntro.Value = value,
                Plugin.AutoSkipIntro.Value,
                AutoSkipIntroDialogueKey);
            
            AddAccessibilityOption(
                "AutoSkipDialogueOption",
                value => Plugin.AutoSkipDialogue.Value = value,
                Plugin.AutoSkipDialogue.Value,
                AutoSkipDialogueDialogueKey);
            
            AddAccessibilityOption(
                "AutoPickupCollectiblesOption",
                value => Plugin.AutoPickupCollectibles.Value = value,
                Plugin.AutoPickupCollectibles.Value,
                AutoPickupCollectiblesDialogueKey);
            
            // var accessibilityOptionPrefab = _settingsManager.cameraFollowsSlopes.transform.parent.gameObject;
            //
            // var option =
            //     Instantiate(accessibilityOptionPrefab, accessibilityOptionPrefab.transform.parent);
            // option.name = "AutoSkipIntroOption";
            //
            // var toggle = option.GetComponentInChildren<Toggle>();
            // toggle.onValueChanged.RemoveAllListeners();
            // toggle.onValueChanged.AddListener(value => Plugin.AutoSkipIntro.Value = value);
            // toggle.isOn = Plugin.AutoSkipIntro.Value;
            //
            // var label = option.GetComponentInChildren<TextLocalisation>();
            // label.keycode = AutoSkipIntroDialogueKey;
            // label.UpdateThisText();

            // we've set up the option and its handler, we don't need to be running Update() anymore.
            gameObject.SetActive(false);
            
            Plugin.Logger.LogDebug($"SettingsInjector: Successfully set up new accessibility options");
        }
        catch (Exception exception)
        {
            Plugin.Logger.LogError(exception);
        }
    }
}