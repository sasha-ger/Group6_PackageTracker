using Moq;
using PackageTracker.Accessors.Interfaces;
using PackageTracker.Engines;
using PackageTracker.Models;

namespace PackageTracker.Tests;

public class RoutingEngineTests
{
    private readonly Mock<IDepotAccessor> _mockDepotAccessor = new();
    private readonly RoutingEngine _engine;

    // Two depots close together (~8.6 miles apart), one far away
    private static readonly Location NearLocation1 = new() { Latitude = 40.0, Longitude = -75.0 };
    private static readonly Location NearLocation2 = new() { Latitude = 40.1, Longitude = -75.1 };
    private static readonly Location FarLocation   = new() { Latitude = 45.0, Longitude = -85.0 };

    private static Depot MakeDepot(int id, Location location) => new()
    {
        Id = id,
        Name = $"Depot {id}",
        Location = location
    };

    public RoutingEngineTests()
    {
        _engine = new RoutingEngine(_mockDepotAccessor.Object);
    }

    // --- GetDistance ---

    [Fact]
    public void GetDistance_SamePoint_ReturnsZero()
    {
        var distance = _engine.GetDistance(40.0, -75.0, 40.0, -75.0);

        Assert.Equal(0.0, distance, precision: 5);
    }

    [Fact]
    public void GetDistance_KnownPoints_ReturnsReasonableDistance()
    {
        // ~8.6 miles between these two points
        var distance = _engine.GetDistance(
            NearLocation1.Latitude, NearLocation1.Longitude,
            NearLocation2.Latitude, NearLocation2.Longitude);

        Assert.InRange(distance, 8.0, 9.5);
    }

    [Fact]
    public void GetDistance_IsSymmetric()
    {
        var d1 = _engine.GetDistance(40.0, -75.0, 40.1, -75.1);
        var d2 = _engine.GetDistance(40.1, -75.1, 40.0, -75.0);

        Assert.Equal(d1, d2, precision: 5);
    }

    // --- IsWithinRange ---

    [Fact]
    public async Task IsWithinRange_LocationNearDepot_ReturnsTrue()
    {
        _mockDepotAccessor.Setup(a => a.GetAll()).ReturnsAsync(new List<Depot>
        {
            MakeDepot(1, NearLocation1)
        });

        // Point 1 mile from depot 1
        var result = await _engine.IsWithinRange(40.0144, -75.0);

        Assert.True(result);
    }

    [Fact]
    public async Task IsWithinRange_LocationFarFromAllDepots_ReturnsFalse()
    {
        _mockDepotAccessor.Setup(a => a.GetAll()).ReturnsAsync(new List<Depot>
        {
            MakeDepot(1, NearLocation1)
        });

        var result = await _engine.IsWithinRange(FarLocation.Latitude, FarLocation.Longitude);

        Assert.False(result);
    }

    [Fact]
    public async Task IsWithinRange_NoDepots_ReturnsFalse()
    {
        _mockDepotAccessor.Setup(a => a.GetAll()).ReturnsAsync(new List<Depot>());

        var result = await _engine.IsWithinRange(40.0, -75.0);

        Assert.False(result);
    }

    // --- FindNearestDepot ---

    [Fact]
    public async Task FindNearestDepot_ReturnsClosestDepotId()
    {
        _mockDepotAccessor.Setup(a => a.GetAll()).ReturnsAsync(new List<Depot>
        {
            MakeDepot(1, NearLocation1),  // close
            MakeDepot(2, FarLocation)     // far
        });

        // Query from a point very close to NearLocation1
        var result = await _engine.FindNearestDepot(40.001, -75.001);

        Assert.Equal(1, result);
    }

    [Fact]
    public async Task FindNearestDepot_SingleDepot_ReturnsThatDepot()
    {
        _mockDepotAccessor.Setup(a => a.GetAll()).ReturnsAsync(new List<Depot>
        {
            MakeDepot(5, FarLocation)
        });

        var result = await _engine.FindNearestDepot(40.0, -75.0);

        Assert.Equal(5, result);
    }

}
