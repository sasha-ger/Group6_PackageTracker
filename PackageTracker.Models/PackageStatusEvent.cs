using PackageTracker.Models.Enums;

namespace PackageTracker.Models;
public class PackageStatusEvent
{
    public int Id { get; set; }
    public int PackageId { get; set; }
    public Package Package { get; set; } = null!;
    public PackageEventType EventType { get; set; }
    public int? DepotId { get; set; }   // null for pickup/delivery events
    public Depot? Depot { get; set; }
    public DateTime Timestamp { get; set; }
}
