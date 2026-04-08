using System;

namespace PackageTracker.Models
{
	public class Drone
	{
		public int Id { get; set; }
		public string status { get; set; }
		public int homeDepot { get; set; }
		public int origin { get; set; }
		public int destination { get; set; }

	}
}
