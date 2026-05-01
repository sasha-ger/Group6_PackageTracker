using Moq;
using PackageTracker.Accessors.Interfaces;
using PackageTracker.Engines;
using PackageTracker.Models;
using PackageTracker.Models.Enums;

namespace PackageTracker.Tests;

public class StaffTrackingEngineTests
{
    private readonly Mock<IDroneAccessor> _mockDroneAccessor = new();
    private readonly Mock<IPackageAccessor> _mockPackageAccessor = new();
    private readonly Mock<IDepotAccessor> _mockDepotAccessor = new();
    private readonly Mock<IRoutingEngine> _mockRoutingEngine = new();
    private readonly StaffTrackingEngine _engine;

    private static Drone MakeDrone(int id, DroneStatus status, int? currentPackageId = null) => new()
    {
        Id = id,
        Status = status,
        HomeDepotId = 1,
        HomeDepot = new Depot { Id = 1, Name = "Home Depot" },
        CurrentPackageId = currentPackageId
    };

    private static Package MakePackage(int id) => new()
    {
        Id = id,
        TrackingNumber = $"TRK{id:D3}",
        Recipient = "Jane Doe",
        Status = PackageStatus.InTransit,
        OriginLocation = new Location { Address = "123 Origin St" },
        DestinationLocation = new Location { Address = "456 Dest Ave" },
        CreatedAt = DateTime.UtcNow
    };

    public StaffTrackingEngineTests()
    {
        _engine = new StaffTrackingEngine(_mockDroneAccessor.Object, _mockPackageAccessor.Object, _mockDepotAccessor.Object, _mockRoutingEngine.Object);
    }

    // --- GetAllDroneStatuses ---

    [Fact]
    public async Task GetAllDroneStatuses_ReturnsDroneList()
    {
        var drones = new List<Drone>
        {
            MakeDrone(1, DroneStatus.Idle),
            MakeDrone(2, DroneStatus.InTransit)
        };
        _mockDroneAccessor.Setup(a => a.GetAll()).ReturnsAsync(drones);

        var result = await _engine.GetAllDroneStatuses();

        Assert.Equal(2, result.Count);
        Assert.Contains(result, d => d.Id == 1 && d.Status == DroneStatus.Idle);
        Assert.Contains(result, d => d.Id == 2 && d.Status == DroneStatus.InTransit);
    }

    [Fact]
    public async Task GetAllDroneStatuses_ReturnsEmptyList_WhenNoDrones()
    {
        _mockDroneAccessor.Setup(a => a.GetAll()).ReturnsAsync(new List<Drone>());

        var result = await _engine.GetAllDroneStatuses();

        Assert.Empty(result);
    }

    // --- GetPackageAssignedToDrone ---

    [Fact]
    public async Task GetPackageAssignedToDrone_DroneNotFound_ThrowsException()
    {
        _mockDroneAccessor.Setup(a => a.GetById(99)).ReturnsAsync((Drone?)null);

        await Assert.ThrowsAsync<Exception>(() => _engine.GetPackageAssignedToDrone(99));
    }

    [Fact]
    public async Task GetPackageAssignedToDrone_NoPackageAssigned_ReturnsNull()
    {
        _mockDroneAccessor.Setup(a => a.GetById(1)).ReturnsAsync(MakeDrone(1, DroneStatus.Idle, currentPackageId: null));

        var result = await _engine.GetPackageAssignedToDrone(1);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetPackageAssignedToDrone_PackageAssigned_ReturnsPackage()
    {
        var drone = MakeDrone(1, DroneStatus.InTransit, currentPackageId: 42);
        var package = MakePackage(42);
        _mockDroneAccessor.Setup(a => a.GetById(1)).ReturnsAsync(drone);
        _mockPackageAccessor.Setup(a => a.GetById(42)).ReturnsAsync(package);

        var result = await _engine.GetPackageAssignedToDrone(1);

        Assert.NotNull(result);
        Assert.Equal(42, result.Id);
    }

    // --- GetAllActivePackages ---

    [Fact]
    public async Task GetAllActivePackages_ReturnsActivePackages()
    {
        var packages = new List<Package> { MakePackage(1), MakePackage(2) };
        _mockPackageAccessor.Setup(a => a.GetAllActive()).ReturnsAsync(packages);

        var result = await _engine.GetAllActivePackages();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetAllActivePackages_ReturnsEmptyList_WhenNoActivePackages()
    {
        _mockPackageAccessor.Setup(a => a.GetAllActive()).ReturnsAsync(new List<Package>());

        var result = await _engine.GetAllActivePackages();

        Assert.Empty(result);
    }
}
