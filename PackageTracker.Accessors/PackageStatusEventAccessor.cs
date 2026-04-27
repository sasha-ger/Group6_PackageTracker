using Microsoft.EntityFrameworkCore;
using PackageTracker.Accessors.Data;
using PackageTracker.Accessors.Interfaces;
using PackageTracker.Models;

namespace PackageTracker.Accessors;
public class PackageStatusEventAccessor : IPackageStatusEventAccessor
{
    private readonly AppDbContext _db;

    public PackageStatusEventAccessor (AppDbContext db)
    {
        _db = db;
    }

    public async Task Create(PackageStatusEvent statusEvent)
    {
        _db.PackageStatusEvents.Add(statusEvent);
        await _db.SaveChangesAsync();
    }
    public async Task<List<PackageStatusEvent>> GetByPackageId(int packageId)
    {
        return await _db.PackageStatusEvents
        .Include(e => e.Depot)
        .Where(e => e.PackageId == packageId)
        .OrderBy(e => e.Timestamp)
        .ToListAsync();
    }
}