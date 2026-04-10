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
	public int? CurrentPackageId { get; set; } 
	public Package? CurrentPackage { get; set; }
	public int? DestinationDepotId { get; set; }  // null when heading to a customer location
	public Depot? DestinationDepot { get; set; }
	public DateTime? EstimatedArrivalTime { get; set; }
}
