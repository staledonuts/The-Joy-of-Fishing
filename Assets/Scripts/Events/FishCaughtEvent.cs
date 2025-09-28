using DonutPackage.EventBus;

public struct FishCaughtEvent : IEvent 
{
    public FishStats Fish;
}
