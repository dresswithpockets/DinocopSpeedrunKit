using System;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.Mono;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Accessibility.BE6;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger = null!;
    private GameObject? _introSkipper;
    private GameObject? _dialogueSkipper;

    public Plugin()
    {
        Logger = base.Logger;
    }

    private void Awake()
    {
        // TODO: auto pickup collectibles
        // TODO: auto skip intro cutscene
        // TODO: auto skip dialogue
        // TODO: change FOV

        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
    }

    private void Start()
    {
        _introSkipper = new GameObject();
        _introSkipper.AddComponent<IntroSkipper>();

        _dialogueSkipper = new GameObject();
        _dialogueSkipper.AddComponent<DialogueSkipper>();

        // PlayerInputHandler.instance.controls.Player.DialogueNext.control

    }

    private void OnDestroy()
    {
        Destroy(_introSkipper);
        Destroy(_dialogueSkipper);
    }
}