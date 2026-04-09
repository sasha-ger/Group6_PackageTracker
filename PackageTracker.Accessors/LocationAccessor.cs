using Microsoft.EntityFrameworkCore;
using PackageTracker.Accessors.Data;
using PackageTracker.Accessors.Interfaces;
using PackageTracker.Models;

namespace PackageTracker.Accessors;

public class LocationAccessor : ILocationAccessor
{
    private readonly AppDbContext _db;

    public LocationAccessor(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Location?> GetById(int id)
    {
        return await _db.Locations.FindAsync(id);
    }

    public async Task<Location?> GetByAddress(string address)
    {
        return await _db.Locations.FirstOrDefaultAsync(l => l.Address == address);
    }

    public async Task<Location> Create(Location location)
    {
        _db.Locations.Add(location);
        await _db.SaveChangesAsync();
        return location;
    }
}
