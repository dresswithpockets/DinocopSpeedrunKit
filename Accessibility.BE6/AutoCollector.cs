using UnityEngine;

namespace Accessibility.BE6;

public class AutoCollector : MonoBehaviour
{
    private void FixedUpdate()
    {
        if (!GameManager.instance)
            return;
        
        var ratMovement = GameManager.instance.ratMovement;
        if (!ratMovement)
            return;

        if (ratMovement.teleporting)
            return;

        if (ratMovement.dialoguing)
            return;

        var mouseLook = GameManager.instance.mouseLook;
        if (!mouseLook)
            return;

        if (mouseLook.inLoupeMode || mouseLook.inLongueVueMode || mouseLook.inFlashLightMode)
            return;

        if (mouseLook.currentDetectable)
            return;

        if (!mouseLook.currentInteractingObject)
            return;

        if (mouseLook.mouseInputDisabled)
            return;

        var castSuccess = Physics.SphereCast(
            mouseLook.camera.transform.position,
            mouseLook.interactionReachRadius,
            mouseLook.camera.transform.forward,
            out var hitInfo,
            mouseLook.interactionReach,
            LayerMask.GetMask("Interactive"));

        if (!castSuccess)
            return;

        var trigger = hitInfo.collider.gameObject.GetComponent<InteractionTrigger>();
        if (!trigger)
            return;

        if (trigger.playerCantTriggerIt)
            return;

        var inventory = Inventaire.instance;
        if (!inventory)
            return;

        // don't auto pickup things with holdableInteraction
        if (trigger.holdableInteraction)
            return;

        if (trigger.inactiveIfNPCMoving && trigger.NavMeshAgentIsMoving())
            return;

        if (trigger.CancelIfNPCKnowPlayerIsHere && trigger.npcDetectBrain.knowThatPlayerIsHere)
            return;

        if (trigger.type == "Collectible")
            ratMovement.CollectCollectible(hitInfo.collider.gameObject);
    }
}