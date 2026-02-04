using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;

namespace Accessibility.BE6;

public static class Input
{
    public static void Press(ButtonControl control, double time = -1, double timeOffset = 0)
    {
        Set(control, 1f, time, timeOffset, true);
    }
    
    public static void Release(ButtonControl control, double time = -1, double timeOffset = 0)
    {
        Set(control, 0f, time, timeOffset, false);
    }
    
    public static void PressAndRelease(ButtonControl control, double time = -1, double timeOffset = 0)
    {
        Set(control, 1f, time, timeOffset, true);
        Set(control, 0f, time, timeOffset, true);
    }
    
    public static void Set<TValue>(InputControl<TValue> control, TValue state, double time = -1, double timeOffset = 0, bool queueEventOnly = false)
        where TValue : struct
    {
        using var _ = DeltaStateEvent.From(control, out var eventPtr);
        eventPtr.time = (time >= 0 ? time : InputState.currentTime) + timeOffset;
        control.WriteValueIntoEvent(state, eventPtr);
        InputSystem.QueueEvent(eventPtr);
        if (!queueEventOnly)
            InputSystem.Update();
    }

    public static void Set<TValue>(InputDevice device, TValue state, double time = -1, double timeOffset = 0,  bool queueEventOnly = false)
        where TValue : struct, IInputStateTypeInfo
    {
        InputSystem.QueueStateEvent(device, state, time);
        if (!queueEventOnly)
            InputSystem.Update();
    }
}