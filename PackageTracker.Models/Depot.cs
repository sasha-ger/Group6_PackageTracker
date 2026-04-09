using System;

namespace PackageTracker.Models;
public class Depot
{
	public int Id { get; set; }
	public string Name { get; set; } = null!; 
	public int LocationId { get; set; } // Foreign key to Location.Id
	public Location Location { get; set; } = null!;
}
