using DonutPackage.EventBus;

public struct PauseStateChangedEvent : IEvent
{
    public bool IsPaused;
}
