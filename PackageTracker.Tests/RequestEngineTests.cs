using Moq;
using PackageTracker.Accessors.Interfaces;
using PackageTracker.Engines;
using PackageTracker.Models;
using PackageTracker.Models.Enums;

namespace PackageTracker.Tests;

public class RequestEngineTests
{
    private readonly Mock<IRoutingEngine> _mockRoutingEngine = new();
    private readonly Mock<ILocationAccessor> _mockLocationAccessor = new();
    private readonly Mock<IPackageAccessor> _mockPackageAccessor = new();
    private readonly Mock<IDroneAccessor> _mockDroneAccessor = new();
    private readonly Mock<IDepotAccessor> _mockDepotAccessor = new();
    private readonly Mock<IPackageStatusEventAccessor> _mockEventAccessor = new();
    private readonly RequestEngine _engine;

    // Shared test doubles
    private static readonly Location OriginLocation      = new() { Id = 1, Address = "123 Origin St", Latitude = 40.0, Longitude = -75.0 };
    private static readonly Location DestinationLocation = new() { Id = 2, Address = "456 Dest Ave",  Latitude = 40.1, Longitude = -75.1 };
    private static readonly Depot    OriginDepot         = new() { Id = 1, Name = "Origin Depot", Location = OriginLocation };

    private static Drone MakeIdleDrone(int id) => new()
    {
        Id = id,
        Status = DroneStatus.Idle,
        HomeDepotId = 1,
        HomeDepot = OriginDepot,
        CurrentDepotId = 1,
        CurrentDepot = OriginDepot
    };

    private static Package MakePackage(int id) => new()
    {
        Id = id,
        TrackingNumber = "ABC12345",
        SenderId = 1,
        Recipient = "Jane Doe",
        OriginLocationId = 1,
        DestinationLocationId = 2,
        Status = PackageStatus.Pending,
        CreatedAt = DateTime.UtcNow
    };

    public RequestEngineTests()
    {
        _engine = new RequestEngine(
            _mockRoutingEngine.Object,
            _mockLocationAccessor.Object,
            _mockPackageAccessor.Object,
            _mockDroneAccessor.Object,
            _mockDepotAccessor.Object,
            _mockEventAccessor.Object);
    }

    // --- ValidateDeliveryLocations ---

    [Fact]
    public async Task ValidateDeliveryLocations_BothInRange_ReturnsTrue()
    {
        _mockRoutingEngine.Setup(r => r.IsWithinRange(40.0, -75.0)).ReturnsAsync(true);
        _mockRoutingEngine.Setup(r => r.IsWithinRange(40.1, -75.1)).ReturnsAsync(true);

        var result = await _engine.ValidateDeliveryLocations(40.0, -75.0, 40.1, -75.1);

        Assert.True(result);
    }

    [Fact]
    public async Task ValidateDeliveryLocations_OriginOutOfRange_ReturnsFalse()
    {
        _mockRoutingEngine.Setup(r => r.IsWithinRange(99.0, -99.0)).ReturnsAsync(false);
        _mockRoutingEngine.Setup(r => r.IsWithinRange(40.1, -75.1)).ReturnsAsync(true);

        var result = await _engine.ValidateDeliveryLocations(99.0, -99.0, 40.1, -75.1);

        Assert.False(result);
    }

    [Fact]
    public async Task ValidateDeliveryLocations_DestinationOutOfRange_ReturnsFalse()
    {
        _mockRoutingEngine.Setup(r => r.IsWithinRange(40.0, -75.0)).ReturnsAsync(true);
        _mockRoutingEngine.Setup(r => r.IsWithinRange(99.0, -99.0)).ReturnsAsync(false);

        var result = await _engine.ValidateDeliveryLocations(40.0, -75.0, 99.0, -99.0);

        Assert.False(result);
    }

    [Fact]
    public async Task ValidateDeliveryLocations_BothOutOfRange_ReturnsFalse()
    {
        _mockRoutingEngine.Setup(r => r.IsWithinRange(It.IsAny<double>(), It.IsAny<double>())).ReturnsAsync(false);

        var result = await _engine.ValidateDeliveryLocations(99.0, -99.0, 88.0, -88.0);

        Assert.False(result);
    }

    // --- ProcessDeliveryRequest ---

    [Fact]
    public async Task ProcessDeliveryRequest_InvalidLocations_ThrowsArgumentException()
    {
        _mockRoutingEngine.Setup(r => r.IsWithinRange(It.IsAny<double>(), It.IsAny<double>())).ReturnsAsync(false);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            _engine.ProcessDeliveryRequest(1, "Origin", 99.0, -99.0, "Dest", 88.0, -88.0, "Jane"));
    }

    [Fact]
    public async Task ProcessDeliveryRequest_NoDronesAvailable_ThrowsException()
    {
        _mockRoutingEngine.Setup(r => r.IsWithinRange(It.IsAny<double>(), It.IsAny<double>())).ReturnsAsync(true);
        _mockLocationAccessor.Setup(a => a.GetByAddress("123 Origin St")).ReturnsAsync(OriginLocation);
        _mockLocationAccessor.Setup(a => a.GetByAddress("456 Dest Ave")).ReturnsAsync(DestinationLocation);
        _mockRoutingEngine.Setup(r => r.FindNearestDepot(40.0, -75.0)).ReturnsAsync(1);
        _mockDepotAccessor.Setup(a => a.GetById(1)).ReturnsAsync(OriginDepot);
        _mockPackageAccessor.Setup(a => a.Create(It.IsAny<Package>())).ReturnsAsync(MakePackage(1));
        _mockDroneAccessor.Setup(a => a.GetAvailableAtDepot(1)).ReturnsAsync(new List<Drone>());

        await Assert.ThrowsAsync<Exception>(() =>
            _engine.ProcessDeliveryRequest(1, "123 Origin St", 40.0, -75.0, "456 Dest Ave", 40.1, -75.1, "Jane"));
    }

    [Fact]
    public async Task ProcessDeliveryRequest_HappyPath_CreatesPackageAndDispatchesDrone()
    {
        var drone = MakeIdleDrone(1);
        _mockRoutingEngine.Setup(r => r.IsWithinRange(It.IsAny<double>(), It.IsAny<double>())).ReturnsAsync(true);
        _mockLocationAccessor.Setup(a => a.GetByAddress("123 Origin St")).ReturnsAsync(OriginLocation);
        _mockLocationAccessor.Setup(a => a.GetByAddress("456 Dest Ave")).ReturnsAsync(DestinationLocation);
        _mockRoutingEngine.Setup(r => r.FindNearestDepot(40.0, -75.0)).ReturnsAsync(1);
        _mockDepotAccessor.Setup(a => a.GetById(1)).ReturnsAsync(OriginDepot);
        _mockPackageAccessor.Setup(a => a.Create(It.IsAny<Package>())).ReturnsAsync(MakePackage(1));
        _mockDroneAccessor.Setup(a => a.GetAvailableAtDepot(1)).ReturnsAsync(new List<Drone> { drone });
        _mockRoutingEngine.Setup(r => r.GetDistance(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<double>(), It.IsAny<double>())).Returns(5.0);
        _mockRoutingEngine.Setup(r => r.GetTravelTime(5.0)).Returns(TimeSpan.FromMinutes(10));
        _mockDroneAccessor.Setup(a => a.UpdateDrone(It.IsAny<Drone>())).Returns(Task.CompletedTask);
        _mockEventAccessor.Setup(a => a.Create(It.IsAny<PackageStatusEvent>())).Returns(Task.CompletedTask);

        await _engine.ProcessDeliveryRequest(1, "123 Origin St", 40.0, -75.0, "456 Dest Ave", 40.1, -75.1, "Jane");

        _mockPackageAccessor.Verify(a => a.Create(It.IsAny<Package>()), Times.Once);
        _mockDroneAccessor.Verify(a => a.UpdateDrone(It.IsAny<Drone>()), Times.Once);
        _mockEventAccessor.Verify(a => a.Create(It.IsAny<PackageStatusEvent>()), Times.Once);
    }

    [Fact]
    public async Task ProcessDeliveryRequest_LocationNotInDb_CreatesNewLocation()
    {
        var drone = MakeIdleDrone(1);
        _mockRoutingEngine.Setup(r => r.IsWithinRange(It.IsAny<double>(), It.IsAny<double>())).ReturnsAsync(true);
        // Return null so the engine creates new locations
        _mockLocationAccessor.Setup(a => a.GetByAddress(It.IsAny<string>())).ReturnsAsync((Location?)null);
        _mockLocationAccessor.Setup(a => a.Create(It.IsAny<Location>())).ReturnsAsync(OriginLocation);
        _mockRoutingEngine.Setup(r => r.FindNearestDepot(It.IsAny<double>(), It.IsAny<double>())).ReturnsAsync(1);
        _mockDepotAccessor.Setup(a => a.GetById(1)).ReturnsAsync(OriginDepot);
        _mockPackageAccessor.Setup(a => a.Create(It.IsAny<Package>())).ReturnsAsync(MakePackage(1));
        _mockDroneAccessor.Setup(a => a.GetAvailableAtDepot(1)).ReturnsAsync(new List<Drone> { drone });
        _mockRoutingEngine.Setup(r => r.GetDistance(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<double>(), It.IsAny<double>())).Returns(5.0);
        _mockRoutingEngine.Setup(r => r.GetTravelTime(5.0)).Returns(TimeSpan.FromMinutes(10));
        _mockDroneAccessor.Setup(a => a.UpdateDrone(It.IsAny<Drone>())).Returns(Task.CompletedTask);
        _mockEventAccessor.Setup(a => a.Create(It.IsAny<PackageStatusEvent>())).Returns(Task.CompletedTask);

        await _engine.ProcessDeliveryRequest(1, "New Origin", 40.0, -75.0, "New Dest", 40.1, -75.1, "Jane");

        // Create called once for each location (origin + destination)
        _mockLocationAccessor.Verify(a => a.Create(It.IsAny<Location>()), Times.Exactly(2));
    }

    // --- DispatchDrone ---

    [Fact]
    public async Task DispatchDrone_DroneNotFound_ThrowsException()
    {
        _mockDroneAccessor.Setup(a => a.GetById(99)).ReturnsAsync((Drone?)null);

        await Assert.ThrowsAsync<Exception>(() =>
            _engine.DispatchDrone(99, 1, DateTime.UtcNow.AddMinutes(10)));
    }

    [Fact]
    public async Task DispatchDrone_HappyPath_UpdatesDroneWithPackageAndStatus()
    {
        var drone = MakeIdleDrone(1);
        var eta = DateTime.UtcNow.AddMinutes(10);
        _mockDroneAccessor.Setup(a => a.GetById(1)).ReturnsAsync(drone);
        _mockDroneAccessor.Setup(a => a.UpdateDrone(It.IsAny<Drone>())).Returns(Task.CompletedTask);

        await _engine.DispatchDrone(1, 42, eta);

        _mockDroneAccessor.Verify(a => a.UpdateDrone(It.Is<Drone>(d =>
            d.CurrentPackageId == 42 &&
            d.Status == DroneStatus.EnRouteToPickup &&
            d.CurrentDepotId == null &&
            d.EstimatedArrivalTime == eta
        )), Times.Once);
    }
}
