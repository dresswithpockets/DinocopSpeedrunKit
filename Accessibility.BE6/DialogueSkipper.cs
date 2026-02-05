using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Accessibility.BE6;

[HarmonyPatch(typeof(InputAction), "triggered", MethodType.Getter)]
[SuppressMessage("ReSharper", "InconsistentNaming")]
class DialogueSkipperPatch
{
    public static bool Prefix(InputAction __instance, ref bool __result)
    {
        if (GameManager.instance?.currentDialogueManager && (__instance == PlayerInputHandler.instance.controls.Player.DialogueNext || __instance == PlayerInputHandler.instance.controls.Player.Interact))
        {
            __result = true;
            return false;
        }

        return true;
    }
}

public class DialogueSkipper : MonoBehaviour
{
    public void Awake()
    {
        Plugin.Logger.LogDebug($"DialogueSkipper: Awake");
        // DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        Plugin.Logger.LogDebug($"DialogueSkipper: Start");
        Print();
    }

    private void Print()
    {
        if (!PlayerInputHandler.instance)
        {
            Plugin.Logger.LogDebug($"DialogueSkipper: no PlayerInputHandler.instance!");
            return;
        }
        
        if (PlayerInputHandler.instance.controls == null)
        {
            Plugin.Logger.LogDebug($"DialogueSkipper: no PlayerInputHandler.instance.controls!");
            return;
        }
        
        if (PlayerInputHandler.instance.controls.Player.DialogueNext == null)
        {
            Plugin.Logger.LogDebug($"DialogueSkipper: no PlayerInputHandler.instance.controls.Player.DialogueNext!");
            return;
        }
        
        var inputType = PlayerInputHandler.instance.controls.Player.DialogueNext.GetType();
        var name = PlayerInputHandler.instance.controls.Player.DialogueNext.name;
        var guid = PlayerInputHandler.instance.controls.Player.DialogueNext.id;
        Plugin.Logger.LogDebug($"DialogueSkipper: {inputType}, {name}, {guid}");
    }

    private void Update()
    {
        if (GameManager.instance.currentDialogueManager || GameManager.instance.generalDialogueManager.dialoguing)
        {
            // Print();
        }
        
        // Input.PressAndRelease(InputSystem.GetDevice<Keyboard>().eKey);

        // var buttonControl = PlayerInputHandler.instance.controls.Player.DialogueNext.controls
        //     .OfType<ButtonControl>()
        //     .FirstOrDefault();
        //
        // if (buttonControl == null)
        //     return;
        //
        // Input.PressAndRelease(buttonControl);
    }
}