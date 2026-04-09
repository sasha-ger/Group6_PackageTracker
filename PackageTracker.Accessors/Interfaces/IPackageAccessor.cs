using PackageTracker.Models;
using PackageTracker.Models.Enums;

namespace PackageTracker.Accessors.Interfaces;

public interface IPackageAccessor
{
    Task<Package?> GetById(int id);
    Task<Package?> GetByTrackingNumber(string trackingNumber);
    Task<List<Package>> GetAllActive();
    Task<Package> Create(Package package);
    Task UpdateStatus(int id, PackageStatus status);
}
