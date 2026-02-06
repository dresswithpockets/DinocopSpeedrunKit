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
