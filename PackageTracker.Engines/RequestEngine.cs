namespace PackageTracker.Engines;
using PackageTracker.Accessors.Interfaces;
using PackageTracker.Models;
using PackageTracker.Models.Enums;

public class RequestEngine : IRequestEngine
{
    private readonly IRoutingEngine _routingEngine;
    private readonly ILocationAccessor _locationAccessor;
    private readonly IPackageAccessor _packageAccessor;
    private readonly IDroneAccessor _droneAccessor;
    private readonly IDepotAccessor _depotAccessor;
    private readonly IPackageStatusEventAccessor _eventAccessor;

    public RequestEngine(
        IRoutingEngine routingEngine,
        ILocationAccessor locationAccessor,
        IPackageAccessor packageAccessor,
        IDroneAccessor droneAccessor,
        IDepotAccessor depotAccessor,
        IPackageStatusEventAccessor eventAccessor)
    {
        _routingEngine = routingEngine;
        _locationAccessor = locationAccessor;
        _packageAccessor = packageAccessor;
        _droneAccessor = droneAccessor;
        _depotAccessor = depotAccessor;
        _eventAccessor = eventAccessor;
    }

    public async Task ProcessDeliveryRequest(int customerId, string originAddress, double originLat, double originLng, string destinationAddress, double destinationLat, double destinationLng, string recipient)
    {
        var validLocations = await ValidateDeliveryLocations(originLat, originLng, destinationLat, destinationLng);

        if (!validLocations)
        {
            throw new ArgumentException("Invalid delivery locations.");
        }

        var originLocation = await GetOrCreateLocation(originAddress, originLat, originLng);

        var destinationLocation = await GetOrCreateLocation(destinationAddress, destinationLat, destinationLng);

        var originDepotId = await _routingEngine.FindNearestDepot(originLat, originLng);

        var originDepot = await _depotAccessor.GetById(originDepotId) ?? throw new Exception("No depot found near origin location.");

        var package = await _packageAccessor.Create(new Package
        {
            TrackingNumber = Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
            SenderId = customerId,
            Recipient = recipient,
            OriginLocationId = originLocation.Id,
            DestinationLocationId = destinationLocation.Id,
            Status = PackageStatus.Pending,
            CreatedAt = DateTime.UtcNow,
        });

        await DispatchPickupDrone(package, originDepot, originLat, originLng);

        await _eventAccessor.Create(new PackageStatusEvent
        {
            PackageId = package.Id,
            EventType = PackageEventType.Dispatched,
            DepotId   = originDepotId,
            Timestamp = DateTime.UtcNow
        });
    }

    public async Task<bool> ValidateDeliveryLocations(double originLat, double originLng, double destinationLat, double destinationLng)
    {
        var originValid = await _routingEngine.IsWithinRange(originLat, originLng);
        var destinationValid = await _routingEngine.IsWithinRange(destinationLat, destinationLng);

        return originValid && destinationValid;
    }

    public async Task DispatchDrone(int droneId, int packageId, DateTime estimatedArrivalTime)
    {
        var drone = await _droneAccessor.GetById(droneId) ?? throw new Exception($"Drone {droneId} not found.");

        drone.Status = DroneStatus.EnRouteToPickup;
        drone.CurrentPackageId = packageId;
        drone.CurrentDepotId = null;   // drone has left the depot
        drone.DestinationDepotId = null;   // heading to customer, not a depot
        drone.EstimatedArrivalTime = estimatedArrivalTime;

        await _droneAccessor.UpdateDrone(drone);
    }

    private async Task<Location> GetOrCreateLocation(string address, double lat, double lng)
    {
        var location = await _locationAccessor.GetByAddress(address);

        if (location == null)
        {
            location = await _locationAccessor.Create(new Location
            {
                Address = address,
                Latitude = lat,
                Longitude = lng
            });
        }

        return location;
    }

    private async Task DispatchPickupDrone(Package package, Depot originDepot, double pickupLat, double pickupLng)
    {
        var availableDrones = await _droneAccessor.GetAvailableAtDepot(originDepot.Id);

        if (availableDrones.Count == 0)
        {
            throw new Exception("No available drones at origin depot for pickup.");
        }

        var drone = availableDrones.First();

        var distance = _routingEngine.GetDistance(originDepot.Location.Latitude, originDepot.Location.Longitude, pickupLat, pickupLng);

        var eta = DateTime.UtcNow + _routingEngine.GetTravelTime(distance);

        drone.Status = DroneStatus.EnRouteToPickup;
        drone.CurrentPackageId = package.Id;
        drone.CurrentDepotId = null;   // drone has left the depot
        drone.DestinationDepotId = null;   // heading to pickup location, not a depot
        drone.EstimatedArrivalTime = eta;

        await _droneAccessor.UpdateDrone(drone);
    }
}
