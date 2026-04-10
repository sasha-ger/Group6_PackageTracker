namespace PackageTracker.Models.Enums;
public enum DroneStatus
{
    Idle, // at depot, available for assignment
    EnRouteToPickup, // assigned to a package and en route to pickup location
    InTransit, // carrying package between depots or to destination
    Charging,
    Maintenance
}
