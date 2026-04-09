using PackageTracker.Accessors.Interfaces;
using PackageTracker.Models;

namespace PackageTracker.Engines;

public class StaffTrackingEngine(IDroneAccessor droneAccessor, IPackageAccessor packageAccessor) : IStaffTrackingEngine
{
    public async Task<List<Drone>> GetAllDroneStatuses()
    {
        return await droneAccessor.GetAll();
    }

    public Task<Package?> GetPackageAssignedToDrone(int droneId)
    {
        // TODO: Cannot implement until the schema links a drone to a package.
        // Should probably add CurrentPackageID to Drone in DB
        throw new NotImplementedException();
    }

    public async Task<List<Package>> GetAllActivePackages()
    {
        return await packageAccessor.GetAllActive();
    }
}
