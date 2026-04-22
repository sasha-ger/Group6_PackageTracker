using Moq;
using PackageTracker.Accessors.Interfaces;
using PackageTracker.Engines;
using PackageTracker.Models;
using PackageTracker.Models.Enums;

namespace PackageTracker.Tests;

public class SimulationEngineTests
{
    private readonly Mock<IPackageAccessor> _mockPackageAccessor = new();
    private readonly Mock<IDroneAccessor> _mockDroneAccessor = new();
    private readonly Mock<IDepotAccessor> _mockDepotAccessor = new();
    private readonly Mock<IRoutingEngine> _mockRoutingEngine = new();
    private readonly Mock<IPackageStatusEventAccessor> _mockEventAccessor = new();
    private readonly SimulationEngine _engine;

    public SimulationEngineTests()
    {
        _engine = new SimulationEngine(
            _mockPackageAccessor.Object,
            _mockDroneAccessor.Object,
            _mockDepotAccessor.Object,
            _mockRoutingEngine.Object,
            _mockEventAccessor.Object);
    }

    private static Location MakeLocation(int id, double lat = 40.0, double lng = -75.0) => new()
    {
        Id = id,
        Address = $"Address {id}",
        Latitude = lat,
        Longitude = lng
    };

    private static Depot MakeDepot(int id, Location location) => new()
    {
        Id = id,
        Name = $"Depot {id}",
        LocationId = location.Id,
        Location = location
    };

    private static Package MakePackage(int id, Location origin, Location destination) => new()
    {
        Id = id,
        TrackingNumber = $"TRK{id:D3}",
        SenderId = 1,
        Recipient = "Jane Doe",
        OriginLocationId = origin.Id,
        OriginLocation = origin,
        DestinationLocationId = destination.Id,
        DestinationLocation = destination,
        Status = PackageStatus.InTransit,
        CreatedAt = DateTime.UtcNow
    };

    private static Drone MakeDrone(int id, DroneStatus status, int homeDepotId,
        int? packageId = null, int? destDepotId = null, DateTime? eta = null) => new()
    {
        Id = id,
        Status = status,
        HomeDepotId = homeDepotId,
        CurrentPackageId = packageId,
        DestinationDepotId = destDepotId,
        EstimatedArrivalTime = eta
    };

    // --- Basic TickAsync cases ---

    [Fact]
    public async Task TickAsync_NoActivePackagesAndNoDrones_DoesNothing()
    {
        _mockPackageAccessor.Setup(a => a.GetActivePackagesWithLocations()).ReturnsAsync(new List<Package>());
        _mockDroneAccessor.Setup(a => a.GetAll()).ReturnsAsync(new List<Drone>());

        await _engine.TickAsync();

        _mockDroneAccessor.Verify(a => a.UpdateDrone(It.IsAny<Drone>()), Times.Never);
        _mockPackageAccessor.Verify(a => a.UpdateStatus(It.IsAny<int>(), It.IsAny<PackageStatus>()), Times.Never);
        _mockEventAccessor.Verify(a => a.Create(It.IsAny<PackageStatusEvent>()), Times.Never);
    }

    [Fact]
    public async Task TickAsync_PackageHasNoDrone_IsSkipped()
    {
        var package = MakePackage(1, MakeLocation(1), MakeLocation(2, 40.1, -75.1));

        _mockPackageAccessor.Setup(a => a.GetActivePackagesWithLocations()).ReturnsAsync(new List<Package> { package });
        _mockDroneAccessor.Setup(a => a.GetByCurrentPackageId(1)).ReturnsAsync((Drone?)null);
        _mockDroneAccessor.Setup(a => a.GetAll()).ReturnsAsync(new List<Drone>());

        await _engine.TickAsync();

        _mockPackageAccessor.Verify(a => a.UpdateStatus(It.IsAny<int>(), It.IsAny<PackageStatus>()), Times.Never);
        _mockEventAccessor.Verify(a => a.Create(It.IsAny<PackageStatusEvent>()), Times.Never);
    }

    [Fact]
    public async Task TickAsync_DroneEtaNotYetPassed_IsSkipped()
    {
        var package = MakePackage(1, MakeLocation(1), MakeLocation(2, 40.1, -75.1));
        var drone = MakeDrone(1, DroneStatus.EnRouteToPickup, homeDepotId: 1, packageId: 1,
            eta: DateTime.UtcNow.AddHours(1));

        _mockPackageAccessor.Setup(a => a.GetActivePackagesWithLocations()).ReturnsAsync(new List<Package> { package });
        _mockDroneAccessor.Setup(a => a.GetByCurrentPackageId(1)).ReturnsAsync(drone);
        _mockDroneAccessor.Setup(a => a.GetAll()).ReturnsAsync(new List<Drone>());

        await _engine.TickAsync();

        _mockPackageAccessor.Verify(a => a.UpdateStatus(It.IsAny<int>(), It.IsAny<PackageStatus>()), Times.Never);
        _mockEventAccessor.Verify(a => a.Create(It.IsAny<PackageStatusEvent>()), Times.Never);
    }

    // --- Returning drone cleanup ---

    [Fact]
    public async Task TickAsync_ReturningDrone_EtaPassed_BecomesIdleAtHomeDepot()
    {
        var returningDrone = MakeDrone(5, DroneStatus.InTransit, homeDepotId: 3,
            packageId: null, destDepotId: 3, eta: DateTime.UtcNow.AddMinutes(-5));

        _mockPackageAccessor.Setup(a => a.GetActivePackagesWithLocations()).ReturnsAsync(new List<Package>());
        _mockDroneAccessor.Setup(a => a.GetAll()).ReturnsAsync(new List<Drone> { returningDrone });
        _mockDroneAccessor.Setup(a => a.UpdateDrone(It.IsAny<Drone>())).Returns(Task.CompletedTask);

        await _engine.TickAsync();

        _mockDroneAccessor.Verify(a => a.UpdateDrone(It.Is<Drone>(d =>
            d.Id == 5 &&
            d.Status == DroneStatus.Idle &&
            d.CurrentDepotId == 3 &&
            d.DestinationDepotId == null &&
            d.EstimatedArrivalTime == null
        )), Times.Once);
    }

    [Fact]
    public async Task TickAsync_ReturningDrone_EtaNotPassed_NotUpdated()
    {
        var returningDrone = MakeDrone(5, DroneStatus.InTransit, homeDepotId: 3,
            packageId: null, destDepotId: 3, eta: DateTime.UtcNow.AddMinutes(5));

        _mockPackageAccessor.Setup(a => a.GetActivePackagesWithLocations()).ReturnsAsync(new List<Package>());
        _mockDroneAccessor.Setup(a => a.GetAll()).ReturnsAsync(new List<Drone> { returningDrone });

        await _engine.TickAsync();

        _mockDroneAccessor.Verify(a => a.UpdateDrone(It.IsAny<Drone>()), Times.Never);
    }

    [Fact]
    public async Task TickAsync_InTransitDroneWithPackage_NotTreatedAsReturning()
    {
        // A drone still carrying a package should not be landed by the returning-drone cleanup loop
        var activeDrone = MakeDrone(7, DroneStatus.InTransit, homeDepotId: 1,
            packageId: 99, destDepotId: 2, eta: DateTime.UtcNow.AddMinutes(-1));

        _mockPackageAccessor.Setup(a => a.GetActivePackagesWithLocations()).ReturnsAsync(new List<Package>());
        _mockDroneAccessor.Setup(a => a.GetAll()).ReturnsAsync(new List<Drone> { activeDrone });

        await _engine.TickAsync();

        _mockDroneAccessor.Verify(a => a.UpdateDrone(It.IsAny<Drone>()), Times.Never);
    }

    // --- EnRouteToPickup: package pickup ---

    [Fact]
    public async Task TickAsync_EnRouteToPickup_DirectDelivery_PicksUpAndDispatchesDeliveryDroneToAddress()
    {
        var origin = MakeLocation(1, 40.0, -75.0);
        var dest = MakeLocation(2, 40.05, -75.05);
        var package = MakePackage(1, origin, dest);

        // Pickup drone: arrived at origin, ETA passed
        var pickupDrone = MakeDrone(10, DroneStatus.EnRouteToPickup, homeDepotId: 1,
            packageId: 1, eta: DateTime.UtcNow.AddMinutes(-1));
        // Delivery drone waiting at origin depot
        var deliveryDrone = MakeDrone(11, DroneStatus.Idle, homeDepotId: 1);
        var depot1 = MakeDepot(1, origin);

        _mockPackageAccessor.Setup(a => a.GetActivePackagesWithLocations()).ReturnsAsync(new List<Package> { package });
        _mockDroneAccessor.Setup(a => a.GetByCurrentPackageId(1)).ReturnsAsync(pickupDrone);
        _mockDroneAccessor.Setup(a => a.GetAll()).ReturnsAsync(new List<Drone>());

        _mockPackageAccessor.Setup(a => a.UpdateStatus(1, PackageStatus.InTransit)).Returns(Task.CompletedTask);
        _mockEventAccessor.Setup(a => a.Create(It.IsAny<PackageStatusEvent>())).Returns(Task.CompletedTask);

        // Origin and destination both nearest to depot 1 → single-element route = direct delivery
        _mockRoutingEngine.Setup(r => r.FindNearestDepot(origin.Latitude, origin.Longitude)).ReturnsAsync(1);
        _mockRoutingEngine.Setup(r => r.FindNearestDepot(dest.Latitude, dest.Longitude)).ReturnsAsync(1);
        _mockRoutingEngine.Setup(r => r.FindShortestRoute(1, 1)).ReturnsAsync(new List<int> { 1 });

        _mockDroneAccessor.Setup(a => a.GetAvailableAtDepot(1)).ReturnsAsync(new List<Drone> { deliveryDrone });
        _mockDepotAccessor.Setup(a => a.GetById(1)).ReturnsAsync(depot1);
        _mockRoutingEngine.Setup(r => r.GetDistance(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<double>(), It.IsAny<double>())).Returns(5.0);
        _mockRoutingEngine.Setup(r => r.GetTravelTime(5.0)).Returns(TimeSpan.FromMinutes(10));
        _mockDroneAccessor.Setup(a => a.UpdateDrone(It.IsAny<Drone>())).Returns(Task.CompletedTask);

        await _engine.TickAsync();

        // Package status updated to InTransit and PickedUp event fired
        _mockPackageAccessor.Verify(a => a.UpdateStatus(1, PackageStatus.InTransit), Times.Once);
        _mockEventAccessor.Verify(a => a.Create(It.Is<PackageStatusEvent>(e =>
            e.EventType == PackageEventType.PickedUp && e.PackageId == 1
        )), Times.Once);

        // Delivery drone dispatched to customer address (DestinationDepotId == null)
        _mockDroneAccessor.Verify(a => a.UpdateDrone(It.Is<Drone>(d =>
            d.Id == 11 &&
            d.Status == DroneStatus.InTransit &&
            d.CurrentPackageId == 1 &&
            d.DestinationDepotId == null
        )), Times.Once);

        // Pickup drone sent back to home depot
        _mockDroneAccessor.Verify(a => a.UpdateDrone(It.Is<Drone>(d =>
            d.Id == 10 &&
            d.Status == DroneStatus.InTransit &&
            d.CurrentPackageId == null &&
            d.DestinationDepotId == 1
        )), Times.Once);
    }

    [Fact]
    public async Task TickAsync_EnRouteToPickup_MultiHopDelivery_DispatchesDeliveryDroneToRelayDepot()
    {
        var origin = MakeLocation(1, 40.0, -75.0);
        var dest = MakeLocation(2, 41.0, -76.0);
        var package = MakePackage(1, origin, dest);

        var pickupDrone = MakeDrone(10, DroneStatus.EnRouteToPickup, homeDepotId: 1,
            packageId: 1, eta: DateTime.UtcNow.AddMinutes(-1));
        var deliveryDrone = MakeDrone(11, DroneStatus.Idle, homeDepotId: 1);
        var depot1 = MakeDepot(1, origin);
        var depot2 = MakeDepot(2, MakeLocation(3, 40.5, -75.5));

        _mockPackageAccessor.Setup(a => a.GetActivePackagesWithLocations()).ReturnsAsync(new List<Package> { package });
        _mockDroneAccessor.Setup(a => a.GetByCurrentPackageId(1)).ReturnsAsync(pickupDrone);
        _mockDroneAccessor.Setup(a => a.GetAll()).ReturnsAsync(new List<Drone>());

        _mockPackageAccessor.Setup(a => a.UpdateStatus(1, PackageStatus.InTransit)).Returns(Task.CompletedTask);
        _mockEventAccessor.Setup(a => a.Create(It.IsAny<PackageStatusEvent>())).Returns(Task.CompletedTask);

        // Route needs a relay: origin nearest depot 1, destination nearest depot 2
        _mockRoutingEngine.Setup(r => r.FindNearestDepot(origin.Latitude, origin.Longitude)).ReturnsAsync(1);
        _mockRoutingEngine.Setup(r => r.FindNearestDepot(dest.Latitude, dest.Longitude)).ReturnsAsync(2);
        _mockRoutingEngine.Setup(r => r.FindShortestRoute(1, 2)).ReturnsAsync(new List<int> { 1, 2 });

        _mockDroneAccessor.Setup(a => a.GetAvailableAtDepot(1)).ReturnsAsync(new List<Drone> { deliveryDrone });
        _mockDepotAccessor.Setup(a => a.GetById(1)).ReturnsAsync(depot1);
        _mockDepotAccessor.Setup(a => a.GetById(2)).ReturnsAsync(depot2);
        _mockRoutingEngine.Setup(r => r.GetDistance(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<double>(), It.IsAny<double>())).Returns(8.0);
        _mockRoutingEngine.Setup(r => r.GetTravelTime(8.0)).Returns(TimeSpan.FromMinutes(16));
        _mockDroneAccessor.Setup(a => a.UpdateDrone(It.IsAny<Drone>())).Returns(Task.CompletedTask);

        await _engine.TickAsync();

        // Delivery drone dispatched to depot 2 (relay hop)
        _mockDroneAccessor.Verify(a => a.UpdateDrone(It.Is<Drone>(d =>
            d.Id == 11 &&
            d.Status == DroneStatus.InTransit &&
            d.CurrentPackageId == 1 &&
            d.DestinationDepotId == 2
        )), Times.Once);
    }

    [Fact]
    public async Task TickAsync_EnRouteToPickup_NoAvailableDroneAtDepot_DeliveryNotDispatched()
    {
        var origin = MakeLocation(1, 40.0, -75.0);
        var dest = MakeLocation(2, 40.05, -75.05);
        var package = MakePackage(1, origin, dest);

        var pickupDrone = MakeDrone(10, DroneStatus.EnRouteToPickup, homeDepotId: 1,
            packageId: 1, eta: DateTime.UtcNow.AddMinutes(-1));
        var depot1 = MakeDepot(1, origin);

        _mockPackageAccessor.Setup(a => a.GetActivePackagesWithLocations()).ReturnsAsync(new List<Package> { package });
        _mockDroneAccessor.Setup(a => a.GetByCurrentPackageId(1)).ReturnsAsync(pickupDrone);
        _mockDroneAccessor.Setup(a => a.GetAll()).ReturnsAsync(new List<Drone>());

        _mockPackageAccessor.Setup(a => a.UpdateStatus(1, PackageStatus.InTransit)).Returns(Task.CompletedTask);
        _mockEventAccessor.Setup(a => a.Create(It.IsAny<PackageStatusEvent>())).Returns(Task.CompletedTask);

        _mockRoutingEngine.Setup(r => r.FindNearestDepot(origin.Latitude, origin.Longitude)).ReturnsAsync(1);
        _mockRoutingEngine.Setup(r => r.FindNearestDepot(dest.Latitude, dest.Longitude)).ReturnsAsync(1);
        _mockRoutingEngine.Setup(r => r.FindShortestRoute(1, 1)).ReturnsAsync(new List<int> { 1 });

        // No available drones at depot 1
        _mockDroneAccessor.Setup(a => a.GetAvailableAtDepot(1)).ReturnsAsync(new List<Drone>());
        _mockDepotAccessor.Setup(a => a.GetById(1)).ReturnsAsync(depot1);
        _mockRoutingEngine.Setup(r => r.GetDistance(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<double>(), It.IsAny<double>())).Returns(5.0);
        _mockRoutingEngine.Setup(r => r.GetTravelTime(5.0)).Returns(TimeSpan.FromMinutes(10));
        _mockDroneAccessor.Setup(a => a.UpdateDrone(It.IsAny<Drone>())).Returns(Task.CompletedTask);

        await _engine.TickAsync();

        // Pickup event still fires, but no new drone is dispatched for delivery
        _mockEventAccessor.Verify(a => a.Create(It.Is<PackageStatusEvent>(e =>
            e.EventType == PackageEventType.PickedUp
        )), Times.Once);
        // Only the pickup drone itself is updated (returning home); no delivery drone update
        _mockDroneAccessor.Verify(a => a.UpdateDrone(It.Is<Drone>(d => d.Id == 10)), Times.Once);
    }

    // --- InTransit: relay depot arrival ---

    [Fact]
    public async Task TickAsync_InTransit_ArrivedAtRelayDepot_MoreHopsRemain_DispatchesToNextDepot()
    {
        var origin = MakeLocation(1, 40.0, -75.0);
        var dest = MakeLocation(2, 41.0, -76.0);
        var package = MakePackage(1, origin, dest);

        // Drone just arrived at depot 2, but destination is nearest to depot 3
        var drone = MakeDrone(10, DroneStatus.InTransit, homeDepotId: 1,
            packageId: 1, destDepotId: 2, eta: DateTime.UtcNow.AddMinutes(-1));
        var nextDrone = MakeDrone(20, DroneStatus.Idle, homeDepotId: 2);
        var depot2 = MakeDepot(2, MakeLocation(3, 40.5, -75.5));
        var depot3 = MakeDepot(3, MakeLocation(4, 41.0, -76.0));

        _mockPackageAccessor.Setup(a => a.GetActivePackagesWithLocations()).ReturnsAsync(new List<Package> { package });
        _mockDroneAccessor.Setup(a => a.GetByCurrentPackageId(1)).ReturnsAsync(drone);
        _mockDroneAccessor.Setup(a => a.GetAll()).ReturnsAsync(new List<Drone>());

        _mockEventAccessor.Setup(a => a.Create(It.IsAny<PackageStatusEvent>())).Returns(Task.CompletedTask);
        _mockDroneAccessor.Setup(a => a.UpdateDrone(It.IsAny<Drone>())).Returns(Task.CompletedTask);

        _mockRoutingEngine.Setup(r => r.FindNearestDepot(dest.Latitude, dest.Longitude)).ReturnsAsync(3);
        _mockRoutingEngine.Setup(r => r.FindShortestRoute(2, 3)).ReturnsAsync(new List<int> { 2, 3 });

        _mockDroneAccessor.Setup(a => a.GetAvailableAtDepot(2)).ReturnsAsync(new List<Drone> { nextDrone });
        _mockDepotAccessor.Setup(a => a.GetById(2)).ReturnsAsync(depot2);
        _mockDepotAccessor.Setup(a => a.GetById(3)).ReturnsAsync(depot3);
        _mockRoutingEngine.Setup(r => r.GetDistance(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<double>(), It.IsAny<double>())).Returns(10.0);
        _mockRoutingEngine.Setup(r => r.GetTravelTime(10.0)).Returns(TimeSpan.FromMinutes(20));

        await _engine.TickAsync();

        // ArrivedAtDepot event logged for depot 2
        _mockEventAccessor.Verify(a => a.Create(It.Is<PackageStatusEvent>(e =>
            e.EventType == PackageEventType.ArrivedAtDepot &&
            e.DepotId == 2 &&
            e.PackageId == 1
        )), Times.Once);

        // Original drone released at depot 2
        _mockDroneAccessor.Verify(a => a.UpdateDrone(It.Is<Drone>(d =>
            d.Id == 10 &&
            d.Status == DroneStatus.Idle &&
            d.CurrentPackageId == null &&
            d.CurrentDepotId == 2 &&
            d.DestinationDepotId == null
        )), Times.Once);

        // Next drone dispatched toward depot 3
        _mockDroneAccessor.Verify(a => a.UpdateDrone(It.Is<Drone>(d =>
            d.Id == 20 &&
            d.Status == DroneStatus.InTransit &&
            d.CurrentPackageId == 1 &&
            d.DestinationDepotId == 3
        )), Times.Once);
    }

    [Fact]
    public async Task TickAsync_InTransit_ArrivedAtFinalRelayDepot_DispatchesLastMileDroneToAddress()
    {
        var origin = MakeLocation(1, 40.0, -75.0);
        var dest = MakeLocation(2, 40.1, -75.1);
        var package = MakePackage(1, origin, dest);

        // Drone arrived at depot 2, which is the nearest depot to the destination
        var drone = MakeDrone(10, DroneStatus.InTransit, homeDepotId: 1,
            packageId: 1, destDepotId: 2, eta: DateTime.UtcNow.AddMinutes(-1));
        var deliveryDrone = MakeDrone(20, DroneStatus.Idle, homeDepotId: 2);
        var depot2 = MakeDepot(2, MakeLocation(3, 40.1, -75.1));

        _mockPackageAccessor.Setup(a => a.GetActivePackagesWithLocations()).ReturnsAsync(new List<Package> { package });
        _mockDroneAccessor.Setup(a => a.GetByCurrentPackageId(1)).ReturnsAsync(drone);
        _mockDroneAccessor.Setup(a => a.GetAll()).ReturnsAsync(new List<Drone>());

        _mockEventAccessor.Setup(a => a.Create(It.IsAny<PackageStatusEvent>())).Returns(Task.CompletedTask);
        _mockDroneAccessor.Setup(a => a.UpdateDrone(It.IsAny<Drone>())).Returns(Task.CompletedTask);

        // Depot 2 is also the nearest to destination → route from 2 to 2 = single element
        _mockRoutingEngine.Setup(r => r.FindNearestDepot(dest.Latitude, dest.Longitude)).ReturnsAsync(2);
        _mockRoutingEngine.Setup(r => r.FindShortestRoute(2, 2)).ReturnsAsync(new List<int> { 2 });

        _mockDroneAccessor.Setup(a => a.GetAvailableAtDepot(2)).ReturnsAsync(new List<Drone> { deliveryDrone });
        _mockDepotAccessor.Setup(a => a.GetById(2)).ReturnsAsync(depot2);
        _mockRoutingEngine.Setup(r => r.GetDistance(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<double>(), It.IsAny<double>())).Returns(3.0);
        _mockRoutingEngine.Setup(r => r.GetTravelTime(3.0)).Returns(TimeSpan.FromMinutes(6));

        await _engine.TickAsync();

        // Delivery drone dispatched to customer address (no destination depot)
        _mockDroneAccessor.Verify(a => a.UpdateDrone(It.Is<Drone>(d =>
            d.Id == 20 &&
            d.Status == DroneStatus.InTransit &&
            d.CurrentPackageId == 1 &&
            d.DestinationDepotId == null
        )), Times.Once);
    }

    // --- InTransit: final delivery to customer ---

    [Fact]
    public async Task TickAsync_InTransit_NoDestinationDepot_PackageMarkedDeliveredAndDroneSentHome()
    {
        var origin = MakeLocation(1, 40.0, -75.0);
        var dest = MakeLocation(2, 40.1, -75.1);
        var package = MakePackage(1, origin, dest);

        // Drone heading directly to customer address (no DestinationDepotId)
        var drone = MakeDrone(10, DroneStatus.InTransit, homeDepotId: 1,
            packageId: 1, destDepotId: null, eta: DateTime.UtcNow.AddMinutes(-1));
        var homeDepot = MakeDepot(1, MakeLocation(5, 40.0, -75.0));

        _mockPackageAccessor.Setup(a => a.GetActivePackagesWithLocations()).ReturnsAsync(new List<Package> { package });
        _mockDroneAccessor.Setup(a => a.GetByCurrentPackageId(1)).ReturnsAsync(drone);
        _mockDroneAccessor.Setup(a => a.GetAll()).ReturnsAsync(new List<Drone>());

        _mockPackageAccessor.Setup(a => a.UpdateStatus(1, PackageStatus.Delivered)).Returns(Task.CompletedTask);
        _mockEventAccessor.Setup(a => a.Create(It.IsAny<PackageStatusEvent>())).Returns(Task.CompletedTask);
        _mockDepotAccessor.Setup(a => a.GetById(1)).ReturnsAsync(homeDepot);
        _mockRoutingEngine.Setup(r => r.GetDistance(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<double>(), It.IsAny<double>())).Returns(6.0);
        _mockRoutingEngine.Setup(r => r.GetTravelTime(6.0)).Returns(TimeSpan.FromMinutes(12));
        _mockDroneAccessor.Setup(a => a.UpdateDrone(It.IsAny<Drone>())).Returns(Task.CompletedTask);

        await _engine.TickAsync();

        _mockPackageAccessor.Verify(a => a.UpdateStatus(1, PackageStatus.Delivered), Times.Once);

        _mockEventAccessor.Verify(a => a.Create(It.Is<PackageStatusEvent>(e =>
            e.EventType == PackageEventType.Delivered &&
            e.DepotId == null &&
            e.PackageId == 1
        )), Times.Once);

        // Drone dispatched back to home depot
        _mockDroneAccessor.Verify(a => a.UpdateDrone(It.Is<Drone>(d =>
            d.Id == 10 &&
            d.Status == DroneStatus.InTransit &&
            d.CurrentPackageId == null &&
            d.DestinationDepotId == 1
        )), Times.Once);
    }

    [Fact]
    public async Task TickAsync_InTransit_NoDestinationDepot_HomeDepotNotFound_ThrowsException()
    {
        var origin = MakeLocation(1, 40.0, -75.0);
        var dest = MakeLocation(2, 40.1, -75.1);
        var package = MakePackage(1, origin, dest);

        var drone = MakeDrone(10, DroneStatus.InTransit, homeDepotId: 99,
            packageId: 1, destDepotId: null, eta: DateTime.UtcNow.AddMinutes(-1));

        _mockPackageAccessor.Setup(a => a.GetActivePackagesWithLocations()).ReturnsAsync(new List<Package> { package });
        _mockDroneAccessor.Setup(a => a.GetByCurrentPackageId(1)).ReturnsAsync(drone);
        _mockDroneAccessor.Setup(a => a.GetAll()).ReturnsAsync(new List<Drone>());

        _mockPackageAccessor.Setup(a => a.UpdateStatus(1, PackageStatus.Delivered)).Returns(Task.CompletedTask);
        _mockEventAccessor.Setup(a => a.Create(It.IsAny<PackageStatusEvent>())).Returns(Task.CompletedTask);
        _mockDepotAccessor.Setup(a => a.GetById(99)).ReturnsAsync((Depot?)null);

        await Assert.ThrowsAsync<Exception>(() => _engine.TickAsync());
    }
}
