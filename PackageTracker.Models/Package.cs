using System;
using PackageTracker.Models.Enums;

namespace PackageTracker.Models;
public class Package
{
    public int Id { get; set; }
    public string TrackingNumber { get; set; } = null!; // Tracking number for the package (e.g. carrier tracking code)
    public int SenderId { get; set; }
    public User Sender { get; set; } = null!;
    public string Recipient { get; set; } = null!;
    public int OriginLocationId { get; set; } // Foreign key to Location.Id
    public Location OriginLocation { get; set; } = null!;
    public int DestinationLocationId { get; set; } // Foreign key to Location.Id
    public Location DestinationLocation { get; set; } = null!;
    public PackageStatus Status { get; set; }
    public DateTime CreatedAt { get; set; } // non-nullable, set when package is created
    public DateTime? UpdatedAt { get; set; } // nullable, only set if/when package status is updated
}
