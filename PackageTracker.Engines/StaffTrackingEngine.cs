using PackageTracker.Accessors.Interfaces;
using PackageTracker.Models;

namespace PackageTracker.Engines;

public class StaffTrackingEngine(IDroneAccessor droneAccessor, IPackageAccessor packageAccessor) : IStaffTrackingEngine
{
    public async Task<List<Drone>> GetAllDroneStatuses()
    {
        return await droneAccessor.GetAll();
    }

    public async Task<Package?> GetPackageAssignedToDrone(int droneId)
    {
        var drone = await droneAccessor.GetById(droneId)
            ?? throw new Exception($"Drone {droneId} not found.");

        if (drone.CurrentPackageId == null)
            return null;

        return await packageAccessor.GetById(drone.CurrentPackageId.Value);
    }

    public async Task<Drone?> GetDroneByPackage(int packageId)
    {
        return await droneAccessor.GetByCurrentPackageId(packageId);
    }

    public async Task<List<Package>> GetAllActivePackages()
    {
        return await packageAccessor.GetAllActive();
    }
}
