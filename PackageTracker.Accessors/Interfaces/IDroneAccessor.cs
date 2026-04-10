using PackageTracker.Models;
using PackageTracker.Models.Enums;

namespace PackageTracker.Accessors.Interfaces;

public interface IDroneAccessor
{
    Task<Drone?> GetById(int id);
    Task<List<Drone>> GetAll();
    Task<List<Drone>> GetAvailableAtDepot(int depotId);
    Task UpdateStatus(int id, DroneStatus status);
    Task UpdateCurrentDepot(int id, int? depotId);
    Task UpdateDrone(Drone drone);
    Task<Drone?> GetByCurrentPackageId(int packageId);
}
