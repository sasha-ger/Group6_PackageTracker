namespace PackageTracker.Engines;

public class RequestEngine : IRequestEngine
{
    public Task ProcessDeliveryRequest(int customerId, string origin, string destination, double weight)
    {
        throw new NotImplementedException();
    }

    public Task<bool> ValidateDeliveryLocations(string origin, string destination)
    {
        throw new NotImplementedException();
    }

    public Task DispatchDrone(int depotId, int packageId)
    {
        throw new NotImplementedException();
    }
}
