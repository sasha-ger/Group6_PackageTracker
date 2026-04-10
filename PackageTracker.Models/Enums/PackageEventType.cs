namespace PackageTracker.Models.Enums;
public enum PackageEventType
{
    Dispatched, // drone dispatched from depot toward pickup
    PickedUp, 
    ArrivedAtDepot, 
    Delivered       
}
