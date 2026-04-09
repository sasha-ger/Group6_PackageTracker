using System;
using PackageTracker.Models.Enums;

namespace PackageTracker.Models;
public class Drone
{
	public int Id { get; set; }
	public DroneStatus Status { get; set; }
	public int HomeDepotId { get; set; } // Foreign key to Depot.Id
	public Depot HomeDepot { get; set; } = null!; // Home location of the drone
	public int? CurrentDepotId { get; set; } // Foreign key to Depot.Id
	public Depot? CurrentDepot { get; set; } // Current location of the drone (can be null if in transit)
}
