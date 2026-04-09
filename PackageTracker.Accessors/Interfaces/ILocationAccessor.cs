using PackageTracker.Models;

namespace PackageTracker.Accessors.Interfaces;

public interface ILocationAccessor
{
    Task<Location?> GetById(int id);
    Task<Location?> GetByAddress(string address);
    Task<Location> Create(Location location);
}
