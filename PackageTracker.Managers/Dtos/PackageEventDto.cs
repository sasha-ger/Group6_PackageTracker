using PackageTracker.Models.Enums;

namespace PackageTracker.Managers.Dtos;

public class PackageEventDto
{
    public PackageEventType EventType { get; set; }
    public DateTime Timestamp { get; set; }
    public int? DepotId { get; set; }
    public string? DepotName { get; set; }
}
