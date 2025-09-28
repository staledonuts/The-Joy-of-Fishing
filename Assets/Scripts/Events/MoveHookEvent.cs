using DonutPackage.EventBus;
using UnityEngine;

public struct MoveHookEvent : IEvent
{
    public Vector2 MoveInput;
}
