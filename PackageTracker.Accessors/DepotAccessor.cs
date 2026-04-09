using Microsoft.EntityFrameworkCore;
using PackageTracker.Accessors.Data;
using PackageTracker.Accessors.Interfaces;
using PackageTracker.Models;

namespace PackageTracker.Accessors;

public class DepotAccessor : IDepotAccessor
{
    private readonly AppDbContext _db;

    public DepotAccessor(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Depot?> GetById(int id)
    {
        return await _db.Depots.Include(d => d.Location).FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task<List<Depot>> GetAll()
    {
        return await _db.Depots.Include(d => d.Location).ToListAsync();
    }
}
