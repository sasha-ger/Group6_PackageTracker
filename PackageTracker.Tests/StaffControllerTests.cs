using Microsoft.AspNetCore.Mvc;
using Moq;
using PackageTracker.Engines;
using PackageTracker.Managers.Controllers;
using PackageTracker.Models;
using PackageTracker.Models.Enums;

namespace PackageTracker.Tests;

public class StaffControllerTests
{
    private readonly Mock<IStaffTrackingEngine> _mockEngine = new();
    private readonly StaffController _controller;

    private static Drone MakeDrone(int id, DroneStatus status) => new()
    {
        Id = id,
        Status = status,
        HomeDepotId = 1,
        HomeDepot = new Depot { Id = 1, Name = "Depot 1" }
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

    public StaffControllerTests()
    {
        _controller = new StaffController(_mockEngine.Object);
    }

    // --- GetAllDrones ---

    [Fact]
    public async Task GetAllDrones_ReturnsDroneList()
    {
        var drones = new List<Drone> { MakeDrone(1, DroneStatus.Idle), MakeDrone(2, DroneStatus.InTransit) };
        _mockEngine.Setup(e => e.GetAllDroneStatuses()).ReturnsAsync(drones);

        var result = await _controller.GetAllDrones();

        var ok = Assert.IsType<OkObjectResult>(result);
        var returned = Assert.IsType<List<Drone>>(ok.Value);
        Assert.Equal(2, returned.Count);
    }

    [Fact]
    public async Task GetAllDrones_ReturnsEmptyList_WhenNoDrones()
    {
        _mockEngine.Setup(e => e.GetAllDroneStatuses()).ReturnsAsync(new List<Drone>());

        var result = await _controller.GetAllDrones();

        var ok = Assert.IsType<OkObjectResult>(result);
        var returned = Assert.IsType<List<Drone>>(ok.Value);
        Assert.Empty(returned);
    }

    // --- GetDroneByPackage ---

    [Fact]
    public async Task GetDroneByPackage_DroneFound_ReturnsOk()
    {
        var drone = MakeDrone(1, DroneStatus.InTransit);
        _mockEngine.Setup(e => e.GetDroneByPackage(42)).ReturnsAsync(drone);

        var result = await _controller.GetDroneByPackage(42);

        var ok = Assert.IsType<OkObjectResult>(result);
        var returned = Assert.IsType<Drone>(ok.Value);
        Assert.Equal(1, returned.Id);
    }

    [Fact]
    public async Task GetDroneByPackage_NoDroneAssigned_ReturnsNotFound()
    {
        _mockEngine.Setup(e => e.GetDroneByPackage(99)).ReturnsAsync((Drone?)null);

        var result = await _controller.GetDroneByPackage(99);

        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Contains("99", notFound.Value?.ToString());
    }

    // --- GetAllActivePackages ---

    [Fact]
    public async Task GetAllActivePackages_ReturnsPackageList()
    {
        var packages = new List<Package> { MakePackage(1), MakePackage(2), MakePackage(3) };
        _mockEngine.Setup(e => e.GetAllActivePackages()).ReturnsAsync(packages);

        var result = await _controller.GetAllActivePackages();

        var ok = Assert.IsType<OkObjectResult>(result);
        var returned = Assert.IsType<List<Package>>(ok.Value);
        Assert.Equal(3, returned.Count);
    }

    [Fact]
    public async Task GetAllActivePackages_ReturnsEmptyList_WhenNoneActive()
    {
        _mockEngine.Setup(e => e.GetAllActivePackages()).ReturnsAsync(new List<Package>());

        var result = await _controller.GetAllActivePackages();

        var ok = Assert.IsType<OkObjectResult>(result);
        var returned = Assert.IsType<List<Package>>(ok.Value);
        Assert.Empty(returned);
    }
}
