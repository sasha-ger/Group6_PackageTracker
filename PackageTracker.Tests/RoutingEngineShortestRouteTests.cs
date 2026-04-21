using Moq;
using PackageTracker.Accessors.Interfaces;
using PackageTracker.Engines;
using PackageTracker.Models;

namespace PackageTracker.Tests;

// Tests for RoutingEngine.FindShortestRoute (Dijkstra's algorithm).
// Basic GetDistance / IsWithinRange / FindNearestDepot tests live in RoutingEngineTests.cs.
public class RoutingEngineShortestRouteTests
{
    private readonly Mock<IDepotAccessor> _mockDepotAccessor = new();
    private readonly RoutingEngine _engine;

    public RoutingEngineShortestRouteTests()
    {
        _engine = new RoutingEngine(_mockDepotAccessor.Object);
    }

    private static Depot MakeDepot(int id, double lat, double lng) => new()
    {
        Id = id,
        Name = $"Depot {id}",
        Location = new Location { Latitude = lat, Longitude = lng }
    };

    // --- FindShortestRoute ---

    [Fact]
    public async Task FindShortestRoute_SameOriginAndDestination_ReturnsSingleElementList()
    {
        _mockDepotAccessor.Setup(a => a.GetAll()).ReturnsAsync(new List<Depot>
        {
            MakeDepot(1, 40.0, -75.0)
        });

        var route = await _engine.FindShortestRoute(1, 1);

        Assert.Single(route);
        Assert.Equal(1, route[0]);
    }

    [Fact]
    public async Task FindShortestRoute_DirectConnection_ReturnsTwoHopPath()
    {
        // Depots ~8.6 miles apart — well within the 15-mile drone range
        _mockDepotAccessor.Setup(a => a.GetAll()).ReturnsAsync(new List<Depot>
        {
            MakeDepot(1, 40.0, -75.0),
            MakeDepot(2, 40.1, -75.1)
        });

        var route = await _engine.FindShortestRoute(1, 2);

        Assert.Equal(2, route.Count);
        Assert.Equal(1, route[0]);
        Assert.Equal(2, route[1]);
    }

    [Fact]
    public async Task FindShortestRoute_DirectConnection_PathIsOrderedOriginToDestination()
    {
        _mockDepotAccessor.Setup(a => a.GetAll()).ReturnsAsync(new List<Depot>
        {
            MakeDepot(1, 40.0, -75.0),
            MakeDepot(2, 40.1, -75.1)
        });

        var route = await _engine.FindShortestRoute(1, 2);

        Assert.Equal(1, route.First());
        Assert.Equal(2, route.Last());
    }

    [Fact]
    public async Task FindShortestRoute_RequiresRelayDepot_ReturnsThreeHopPath()
    {
        // A(40.0,-75.0) → C(40.2,-75.2) is ~17.4 miles (exceeds 15-mile range, no direct hop)
        // B(40.1,-75.1) is ~8.6 miles from both A and C, serving as the relay
        _mockDepotAccessor.Setup(a => a.GetAll()).ReturnsAsync(new List<Depot>
        {
            MakeDepot(1, 40.0, -75.0),   // A
            MakeDepot(2, 40.1, -75.1),   // B (relay)
            MakeDepot(3, 40.2, -75.2)    // C
        });

        var route = await _engine.FindShortestRoute(1, 3);

        Assert.Equal(3, route.Count);
        Assert.Equal(1, route[0]);
        Assert.Equal(2, route[1]);
        Assert.Equal(3, route[2]);
    }

    [Fact]
    public async Task FindShortestRoute_UnreachableDestination_ReturnsOnlyDestination()
    {
        // Two depots separated by hundreds of miles with no relay — destination is unreachable
        _mockDepotAccessor.Setup(a => a.GetAll()).ReturnsAsync(new List<Depot>
        {
            MakeDepot(1, 40.0, -75.0),
            MakeDepot(2, 45.0, -85.0)
        });

        var route = await _engine.FindShortestRoute(1, 2);

        // The implementation returns only [destination] when no path exists
        Assert.Single(route);
        Assert.Equal(2, route[0]);
    }

    [Fact]
    public async Task FindShortestRoute_MultiplePathsExist_ChoosesShortestTotalDistance()
    {
        // A(40.0,-75.0) to D(40.2,-75.2) requires a relay (~17.4 miles, no direct hop).
        // Two relays are available:
        //   B(40.1,-75.1): A-B ~8.6mi, B-D ~8.6mi  → total ~17.2 miles
        //   C(40.1,-75.0): A-C ~6.9mi, C-D ~12.7mi → total ~19.6 miles
        // Dijkstra should route through B (shorter total path).
        _mockDepotAccessor.Setup(a => a.GetAll()).ReturnsAsync(new List<Depot>
        {
            MakeDepot(1, 40.0, -75.0),   // A (origin)
            MakeDepot(2, 40.1, -75.1),   // B (shorter relay)
            MakeDepot(3, 40.1, -75.0),   // C (longer relay)
            MakeDepot(4, 40.2, -75.2)    // D (destination)
        });

        var route = await _engine.FindShortestRoute(1, 4);

        Assert.Equal(1, route[0]);
        Assert.Equal(4, route[^1]);
        Assert.Contains(2, route); // path goes through B
    }

    [Fact]
    public async Task FindShortestRoute_SingleDepotNetwork_SameOriginDestination_ReturnsThatDepot()
    {
        _mockDepotAccessor.Setup(a => a.GetAll()).ReturnsAsync(new List<Depot>
        {
            MakeDepot(7, 40.0, -75.0)
        });

        var route = await _engine.FindShortestRoute(7, 7);

        Assert.Single(route);
        Assert.Equal(7, route[0]);
    }
}
