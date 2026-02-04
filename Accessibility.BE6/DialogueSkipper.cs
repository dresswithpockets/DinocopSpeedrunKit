using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace Accessibility.BE6;

public class DialogueSkipper : MonoBehaviour
{
    public void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if (!GameManager.instance.generalDialogueManager.dialoguing) return;
        
        Input.PressAndRelease(InputSystem.GetDevice<Keyboard>().eKey);

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