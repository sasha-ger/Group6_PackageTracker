using Microsoft.EntityFrameworkCore;
using PackageTracker.Accessors.Data;
using PackageTracker.Accessors.Interfaces;
using PackageTracker.Models;
using PackageTracker.Models.Enums;

namespace PackageTracker.Accessors;

public class PackageAccessor : IPackageAccessor
{
    private readonly AppDbContext _db;

    public PackageAccessor(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Package?> GetById(int id)
    {
        return await _db.Packages.FindAsync(id);
    }

    public async Task<Package?> GetByTrackingNumber(string trackingNumber)
    {
        return await _db.Packages.FirstOrDefaultAsync(p => p.TrackingNumber == trackingNumber);
    }

    public async Task<List<Package>> GetAllActive()
    {
        return await _db.Packages.Where(p => p.Status != PackageStatus.Delivered && p.Status != PackageStatus.Failed).ToListAsync();
    }
    
    public async Task<Package> Create(Package package)
    {
        _db.Packages.Add(package);
        await _db.SaveChangesAsync();
        return package;
    }

    public async Task UpdateStatus(int id, PackageStatus status)
    {
        var package = await _db.Packages.FindAsync(id) ?? throw new Exception("Package not found");
        package.Status = status;
        package.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }
}
