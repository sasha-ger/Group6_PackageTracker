using System;

namespace PackageTracker.Models
{
    public class Package
    {
        public int Id { get; set; }

        // Tracking number for the package (e.g. carrier tracking code)
        public string TrackingNumber { get; set; } = null!;

		// Optional metadata
		public string? Sender { get; set; }
		public string? Recipient { get; set; }
		public string? destination { get; set; }
		public string? origin { get; set; }
		public string? Status { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
