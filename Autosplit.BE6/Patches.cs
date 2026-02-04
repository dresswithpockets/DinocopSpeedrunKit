using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using UnityEngine;

namespace Autosplit;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public static class Patches
{
    [HarmonyPatch(typeof(Inventaire), "AddCollectible")]
    [HarmonyPrefix]
    private static bool PreAddCollectible(Collectible _collectible, float _value = 0.0f)
    {
        Plugin.Logger.LogDebug($"PreAddCollectible: (name: {_collectible.name})");
        Plugin.HandleCollectibleSplit(_collectible);
        return true;
    }
    
    [HarmonyPatch(typeof(Inventaire), "CheckLearnScent")]
    [HarmonyPrefix]
    private static bool PreCheckLearnScent(Collectible _collectible, Inventaire __instance)
    {
        Plugin.Logger.LogDebug($"PreCheckLearnScent: (name: {_collectible.type})");
        if (_collectible.cantLearn || __instance.scentsLearned == null ||
            __instance.scentsLearned.Contains(_collectible.type))
            return true;
        
        Plugin.HandleScentSplit(_collectible);
        return true;
    }

    [HarmonyPatch(typeof(EventInstance), "HandleEvents")]
    [HarmonyPrefix]
    private static bool PreHandleEvents(float _eventDelay, EventInstance __instance)
    {
        var events = string.Join(", ", __instance.events);
        Plugin.Logger.LogDebug($"PreHandleEvents: (name: {__instance.name}, delay: {_eventDelay:.4f}, activeInHierarchy: {__instance.gameObject.activeInHierarchy}, events: [{events}])");
        Plugin.HandleEventSplit(__instance);
        return true;
    }

    // These are experimental and may be useful in the future (I suspect that the Lab level transition is an Exit)
#if false
    [HarmonyPatch(typeof(Exit), "TriggerExit")]
    [HarmonyPrefix]
    private static bool PreTriggerExit(Exit __instance)
    {
        Plugin.Logger.LogDebug($"PreTriggerExit: (exitingLevel: {LevelManager.instance.exitingLevel}, horraire: {__instance.horraire}, CestOuvert(): {__instance.CestOuvert()}, localMode: {__instance.localMode}, globalID: {__instance.globalID}, localID: {__instance.localID}, destinationID: {__instance.destinationID}, goToThisLevel: {__instance.goToThisLevel})");
        return true;
    }

    [HarmonyPatch(typeof(Exit), "TestCollision")]
    [HarmonyPrefix]
    private static bool PreTestCollisionExit(GameObject other, Exit __instance, float ____safetyDelay)
    {
        Plugin.Logger.LogDebug($"PreTestCollisionExit: (exitingLevel: {LevelManager.instance.exitingLevel}, horraire: {__instance.horraire}, CestOuvert(): {__instance.CestOuvert()}, localMode: {__instance.localMode}, globalID: {__instance.globalID}, localID: {__instance.localID}, destinationID: {__instance.destinationID}, goToThisLevel: {__instance.goToThisLevel}, collisionMode: {__instance.collisionMode}, canExit: {LevelManager.instance.canExit}, tag: {other.tag})");
        return true;
    }
    
    [HarmonyPatch(typeof(DialogueManager), "StartDialogue")]
    [HarmonyPrefix]
    private static bool PreStartDialogueDialogueManager(Dialogue _dialogue)
    {
        Plugin.Logger.LogDebug($"PreStartDialogueDialogueManager: (name: {_dialogue})");
        Plugin.HandleDialogueSplit(_dialogue);
        return true;
    }
    
    [HarmonyPatch(typeof(EconomyManager), "GiveEquipementToPlayer")]
    [HarmonyPrefix]
    private static bool PreGiveEquipment(Equipement _equip, bool _big = false, bool _small = false, bool _faster = false)
    {
        Plugin.Logger.LogDebug($"PreGiveEquipment: (name: {_equip})");
        return true;
    }

    [HarmonyPatch(typeof(Inventaire), "AddCollectible")]
    [HarmonyPrefix]
    private static bool PreGiveCollectible(Collectible _collectible, float _value = 0.0f)
    {
        var name = GetScriptableObjectName(_collectible);
        Plugin.Logger.LogDebug($"PreGiveCollectible: (name: {name})");
        return true;
    }
    
    [HarmonyPatch(typeof(EconomyManager), "GiveScentToPlayer")]
    [HarmonyPrefix]
    private static bool PreGiveScent(ScentAsset _scent)
    {
        var name = GetScriptableObjectName(_scent);
        Plugin.Logger.LogDebug($"PreGiveScent: (name: {name})");
        return true;
    }
    
    private static string GetScriptableObjectName<T>(T item) where T : ScriptableObject
    {
        var name = item.ToString();
        var postfixLength = typeof(T).Name.Length + 3;
        return name.Substring(0, name.Length - postfixLength);
    }
#endif
}