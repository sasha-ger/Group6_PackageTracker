using PackageTracker.Models;

namespace PackageTracker.Accessors.Interfaces;
public interface IPackageStatusEventAccessor
{
    Task Create(PackageStatusEvent statusEvent);
    Task<List<PackageStatusEvent>> GetByPackageId(int packageId);
}