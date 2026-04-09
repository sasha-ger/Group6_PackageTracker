using PackageTracker.Models;

namespace PackageTracker.Accessors.Interfaces;

public interface IDepotAccessor
{
    Task<Depot?> GetById(int id);
    Task<List<Depot>> GetAll();
}
