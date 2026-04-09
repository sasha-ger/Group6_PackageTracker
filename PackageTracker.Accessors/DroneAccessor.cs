using Microsoft.EntityFrameworkCore;
using PackageTracker.Accessors.Data;
using PackageTracker.Accessors.Interfaces;
using PackageTracker.Models;
using PackageTracker.Models.Enums;

namespace PackageTracker.Accessors;

public class DroneAccessor : IDroneAccessor
{
    private readonly AppDbContext _db;

    public DroneAccessor(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Drone?> GetById(int id)
    {
        return await _db.Drones.FindAsync(id);
    }

    public async Task<List<Drone>> GetAll()
    {
        return await _db.Drones.Include(d => d.CurrentDepot).Include(d => d.HomeDepot).ToListAsync();
    }

    public async Task<List<Drone>> GetAvailableAtDepot(int depotId)
    {
        return await _db.Drones
            .Where(d => d.CurrentDepotId == depotId && d.Status == DroneStatus.Idle)
            .ToListAsync();
    }

    public async Task UpdateStatus(int id, DroneStatus status)
    {
        var drone = await _db.Drones.FindAsync(id) ?? throw new Exception($"Drone with id {id} not found");
        drone.Status = status;
        await _db.SaveChangesAsync();
    }

    public async Task UpdateCurrentDepot(int id, int? depotId)
    {
        var drone = await _db.Drones.FindAsync(id) ?? throw new Exception($"Drone with id {id} not found");
        drone.CurrentDepotId = depotId;
        await _db.SaveChangesAsync();
    }
}
