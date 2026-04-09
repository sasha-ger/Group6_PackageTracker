using PackageTracker.Models;

namespace PackageTracker.Engines;

public interface IStaffTrackingEngine
{
    Task<List<Drone>> GetAllDroneStatuses();
    Task<Package?> GetPackageAssignedToDrone(int droneId);
    Task<List<Package>> GetAllActivePackages();
}
