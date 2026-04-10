namespace PackageTracker.Engines;
using PackageTracker.Models;

public interface IRequestEngine
{
    Task ProcessDeliveryRequest(int customerId, string originAddress, double originLat, double originLng, string destinationAddress, double destinationLat, double destinationLng, string recipient);
    Task<bool> ValidateDeliveryLocations(double originLat, double originLng, double destinationLat, double destinationLng);
    Task DispatchDrone(int droneId, int packageId, DateTime estimatedArrivalTime);
}
