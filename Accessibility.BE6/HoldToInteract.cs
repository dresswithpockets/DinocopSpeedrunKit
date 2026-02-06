using UnityEngine;

namespace Accessibility.BE6;

class HoldToInteract : MonoBehaviour
{
    private void Update()
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

        // only DeSinteract if the user is not themselves holding down the interact button - we don't want to interrupt
        // that interaction
        if (!Input.GetKeyUp(Plugin.HoldToInteractKey.Value) && mouseLook.interacting && !PlayerInputHandler.instance.controls.Player.Interact.IsInProgress())
        {
            PlayerInputHandler.instance.DeSinteract();
            return;
        }
        
        if (!Input.GetKey(Plugin.HoldToInteractKey.Value))
            return;

        if (mouseLook.interacting)
        {
            PlayerInputHandler.instance.DeSinteract();
        }
        else
        {
            PlayerInputHandler.instance.Interact();
        }
    }
}