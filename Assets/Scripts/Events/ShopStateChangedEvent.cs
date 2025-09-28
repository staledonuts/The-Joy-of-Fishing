using DonutPackage.EventBus;

public struct ShopStateChangedEvent : IEvent
{
    public bool IsShopOpen;
}
