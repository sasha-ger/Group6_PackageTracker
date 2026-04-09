using PackageTracker.Models;

namespace PackageTracker.Engines;

public class StaffTrackingEngine : IStaffTrackingEngine
{
    public Task<List<Drone>> GetAllDroneStatuses()
    {
        throw new NotImplementedException();
    }

    public Task<Package?> GetPackageAssignedToDrone(int droneId)
    {
        throw new NotImplementedException();
    }

    public Task<List<Package>> GetAllActivePackages()
    {
        throw new NotImplementedException();
    }
}
