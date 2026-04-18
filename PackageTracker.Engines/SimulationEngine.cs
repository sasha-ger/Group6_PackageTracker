using PackageTracker.Accessors.Interfaces;
using PackageTracker.Models;
using PackageTracker.Models.Enums;

namespace PackageTracker.Engines;

public class SimulationEngine : ISimulationEngine
{
    private readonly IPackageAccessor _packageAccessor;
    private readonly IDroneAccessor _droneAccessor;
    private readonly IDepotAccessor _depotAccessor;
    private readonly IRoutingEngine _routingEngine;
    private readonly IPackageStatusEventAccessor _eventAccessor;

    public SimulationEngine(
        IPackageAccessor packageAccessor,
        IDroneAccessor droneAccessor,
        IDepotAccessor depotAccessor,
        IRoutingEngine routingEngine,
        IPackageStatusEventAccessor eventAccessor)
    {
        _packageAccessor = packageAccessor;
        _droneAccessor = droneAccessor;
        _depotAccessor = depotAccessor;
        _routingEngine = routingEngine;
        _eventAccessor = eventAccessor;
    }

    // Advances the simulation: processes all packages whose drone ETA has passed, then lands returning drones.
    public async Task TickAsync()
    {
        var activePackages = await _packageAccessor.GetActivePackagesWithLocations();

        foreach (var package in activePackages)
        {
            var drone = await _droneAccessor.GetByCurrentPackageId(package.Id);

            if (drone == null) 
                continue;

            if (drone.EstimatedArrivalTime > DateTime.UtcNow) 
                continue;

            await HandleDroneArrival(drone, package);
        }

        var allDrones = await _droneAccessor.GetAll();

        foreach (var drone in allDrones.Where(d =>
            d.Status == DroneStatus.InTransit &&
            d.CurrentPackageId == null &&
            d.EstimatedArrivalTime <= DateTime.UtcNow))
        {
            drone.Status = DroneStatus.Idle;
            drone.CurrentDepotId = drone.HomeDepotId;
            drone.DestinationDepotId = null;
            drone.EstimatedArrivalTime = null;
            await _droneAccessor.UpdateDrone(drone);
        }
    }

    // Transitions package and drone state based on where the drone just arrived.
    private async Task HandleDroneArrival(Drone drone, Package package)
    {
        switch (drone.Status)
        {
            case DroneStatus.EnRouteToPickup:
                await _packageAccessor.UpdateStatus(package.Id, PackageStatus.InTransit);

                await _eventAccessor.Create(new PackageStatusEvent
                {
                    EventType = PackageEventType.PickedUp,
                    DepotId = null,
                    Timestamp = DateTime.UtcNow,
                    PackageId = package.Id
                });

                var closestToOrigin = await _routingEngine.FindNearestDepot(package.OriginLocation.Latitude, package.OriginLocation.Longitude);
                var closestToDestination = await _routingEngine.FindNearestDepot(package.DestinationLocation.Latitude, package.DestinationLocation.Longitude);
                var route = await _routingEngine.FindShortestRoute(closestToOrigin, closestToDestination);

                if (route.Count > 1)
                    await DispatchDroneToDepot(package, closestToOrigin, route[1]);
                else
                    await DispatchDroneToAddress(package, closestToOrigin);

                var pickupHomeDepot = await _depotAccessor.GetById(drone.HomeDepotId) ?? throw new Exception($"Home depot {drone.HomeDepotId} not found.");

                var returnDist = _routingEngine.GetDistance(
                    package.OriginLocation.Latitude, package.OriginLocation.Longitude,
                    pickupHomeDepot.Location.Latitude, pickupHomeDepot.Location.Longitude);

                drone.CurrentPackageId = null;
                drone.CurrentDepotId = null;
                drone.DestinationDepotId = drone.HomeDepotId;
                drone.Status = DroneStatus.InTransit;
                drone.EstimatedArrivalTime = DateTime.UtcNow + _routingEngine.GetTravelTime(returnDist);
                await _droneAccessor.UpdateDrone(drone);

                break;
            case DroneStatus.InTransit:
                if (drone.DestinationDepotId != null)
                {
                    // Drone arrived at an intermediate or final relay depot
                    var arrivedDepotId = drone.DestinationDepotId.Value;

                    await _eventAccessor.Create(new PackageStatusEvent
                    {
                        EventType = PackageEventType.ArrivedAtDepot,
                        DepotId = arrivedDepotId,
                        Timestamp = DateTime.UtcNow,
                        PackageId = package.Id
                    });

                    drone.CurrentPackageId = null;
                    drone.CurrentDepotId = arrivedDepotId;
                    drone.DestinationDepotId = null;
                    drone.Status = DroneStatus.Idle;
                    await _droneAccessor.UpdateDrone(drone);

                    var destDepotId = await _routingEngine.FindNearestDepot(package.DestinationLocation.Latitude, package.DestinationLocation.Longitude);
                    var remainingRoute = await _routingEngine.FindShortestRoute(arrivedDepotId, destDepotId);

                    if (remainingRoute.Count > 1)
                        await DispatchDroneToDepot(package, arrivedDepotId, remainingRoute[1]);
                    else
                        await DispatchDroneToAddress(package, arrivedDepotId);
                }
                else
                {
                    // Drone arrived at the customer's delivery address
                    await _packageAccessor.UpdateStatus(package.Id, PackageStatus.Delivered);

                    await _eventAccessor.Create(new PackageStatusEvent
                    {
                        EventType = PackageEventType.Delivered,
                        DepotId = null,
                        Timestamp = DateTime.UtcNow,
                        PackageId = package.Id
                    });

                    var homeDepot = await _depotAccessor.GetById(drone.HomeDepotId)
                        ?? throw new Exception($"Home depot {drone.HomeDepotId} not found.");

                    var dist = _routingEngine.GetDistance(
                        package.DestinationLocation.Latitude, package.DestinationLocation.Longitude,
                        homeDepot.Location.Latitude, homeDepot.Location.Longitude);

                    drone.CurrentPackageId = null;
                    drone.CurrentDepotId = null;
                    drone.DestinationDepotId = drone.HomeDepotId;
                    drone.Status = DroneStatus.InTransit;
                    drone.EstimatedArrivalTime = DateTime.UtcNow + _routingEngine.GetTravelTime(dist);
                    await _droneAccessor.UpdateDrone(drone);
                }
                break;
        }
    }

    // Assigns an idle drone at fromDepotId to carry the package to the next depot on the route.
    private async Task DispatchDroneToDepot(Package package, int fromDepotId, int toDepotId)
    {
        var available = await _droneAccessor.GetAvailableAtDepot(fromDepotId);
        if (available.Count == 0) 
            return;

        var drone = available.First();
        var fromDepot = await _depotAccessor.GetById(fromDepotId) ?? throw new Exception($"Depot {fromDepotId} not found.");
        var toDepot = await _depotAccessor.GetById(toDepotId) ?? throw new Exception($"Depot {toDepotId} not found.");

        var dist = _routingEngine.GetDistance(
            fromDepot.Location.Latitude, fromDepot.Location.Longitude,
            toDepot.Location.Latitude, toDepot.Location.Longitude);

        drone.Status = DroneStatus.InTransit;
        drone.CurrentPackageId = package.Id;
        drone.CurrentDepotId = null;
        drone.DestinationDepotId = toDepotId;
        drone.EstimatedArrivalTime = DateTime.UtcNow + _routingEngine.GetTravelTime(dist);
        await _droneAccessor.UpdateDrone(drone);
    }

    // Assigns an idle drone at fromDepotId to carry the package to the customer's delivery address.
    private async Task DispatchDroneToAddress(Package package, int fromDepotId)
    {
        var available = await _droneAccessor.GetAvailableAtDepot(fromDepotId);
        if (available.Count == 0) 
            return;

        var drone = available.First();
        var fromDepot = await _depotAccessor.GetById(fromDepotId) ?? throw new Exception($"Depot {fromDepotId} not found.");

        var dist = _routingEngine.GetDistance(
            fromDepot.Location.Latitude, fromDepot.Location.Longitude,
            package.DestinationLocation.Latitude, package.DestinationLocation.Longitude);

        drone.Status = DroneStatus.InTransit;
        drone.CurrentPackageId = package.Id;
        drone.CurrentDepotId = null;
        drone.DestinationDepotId = null;
        drone.EstimatedArrivalTime = DateTime.UtcNow + _routingEngine.GetTravelTime(dist);
        await _droneAccessor.UpdateDrone(drone);
    }
    
}