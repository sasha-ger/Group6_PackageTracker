namespace PackageTracker.Engines;

public interface IRequestEngine
{
    Task ProcessDeliveryRequest(int customerId, string origin, string destination, double weight);
    Task<bool> ValidateDeliveryLocations(string origin, string destination);
    Task DispatchDrone(int depotId, int packageId);
}
