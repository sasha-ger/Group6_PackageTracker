namespace PackageTracker.Managers.Dtos;

public class DeliveryRequestDto
{
    public string OriginAddress { get; set; } = null!;
    public double OriginLat { get; set; }
    public double OriginLng { get; set; }
    public string DestinationAddress { get; set; } = null!;
    public double DestinationLat { get; set; }
    public double DestinationLng { get; set; }
    public string Recipient { get; set; } = null!;
}
