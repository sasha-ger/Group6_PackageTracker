namespace PackageTracker.Engines;
using PackageTracker.Accessors.Interfaces;

public class RoutingEngine : IRoutingEngine
{
    private readonly IDepotAccessor _depotAccessor;
    private const double DroneRangeMiles = 15.0;
    private const double DroneSpeedMph = 30.0;

    public RoutingEngine(IDepotAccessor depotAccessor)
    {
        _depotAccessor = depotAccessor;
    }

    // Calculates the great-circle distance in miles between two lat/lng coordinates using the Haversine formula.
    public double GetDistance(double lat1, double lng1, double lat2, double lng2)
    {
        const double R = 3958.8; // Earth radius in miles

        // convert lat/lng to radians
        var lat1_rad = lat1 * Math.PI / 180;
        var lng1_rad = lng1 * Math.PI / 180;
        var lat2_rad = lat2 * Math.PI / 180;
        var lng2_rad = lng2 * Math.PI / 180;

        var dLat = lat2_rad - lat1_rad;
        var dLng = lng2_rad - lng1_rad;

        var a = Math.Pow(Math.Sin(dLat / 2), 2) + Math.Cos(lat1_rad) * Math.Cos(lat2_rad) * Math.Pow(Math.Sin(dLng / 2), 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return R * c;
    }

    // Returns true if the given location is within drone range of at least one depot.
    public async Task<bool> IsWithinRange(double lat, double lng)
    {
        var depots = await _depotAccessor.GetAll();

        foreach (var depot in depots)
        {
            var distance = GetDistance(lat, lng, depot.Location.Latitude, depot.Location.Longitude);

            if (distance <= DroneRangeMiles)
            {
                return true;
            }
        }
        return false;
    }

    // Returns the ID of the depot closest to the given location.
    public async Task<int> FindNearestDepot(double lat, double lng)
    {
        var depots = await _depotAccessor.GetAll();

        return depots
            .OrderBy(d => GetDistance(lat, lng, d.Location.Latitude, d.Location.Longitude))
            .First().Id;
    }

    // Finds the shortest path between two depots using Dijkstra's algorithm.
    // Returns an ordered list of depot IDs from origin to destination, inclusive.
    public async Task<List<int>> FindShortestRoute(int originDepotId, int destinationDepotId)
    {
        var depots = await _depotAccessor.GetAll();
        var depotMap = depots.ToDictionary(d => d.Id);

        var distances = depots.ToDictionary(d => d.Id, _ => double.MaxValue);
        var previous = depots.ToDictionary(d => d.Id, _ => (int?)null);
        var unvisited = depots.Select(d => d.Id).ToHashSet();

        distances[originDepotId] = 0;

        while (unvisited.Count > 0)
        {
            var current = unvisited.OrderBy(id => distances[id]).First();

            if (current == destinationDepotId) break;
            if (distances[current] == double.MaxValue) break; // unreachable

            unvisited.Remove(current);

            var currentDepot = depotMap[current];

            foreach (var neighborId in unvisited)
            {
                var neighbor = depotMap[neighborId];
                var edgeCost = GetDistance(
                    currentDepot.Location.Latitude, currentDepot.Location.Longitude,
                    neighbor.Location.Latitude, neighbor.Location.Longitude);

                if (edgeCost > DroneRangeMiles) continue; // not connected

                var newCost = distances[current] + edgeCost;
                if (newCost < distances[neighborId])
                {
                    distances[neighborId] = newCost;
                    previous[neighborId] = current;
                }
            }
        }

        // Reconstruct path by walking backwards from destination
        var path = new List<int>();
        int? step = destinationDepotId;
        while (step != null)
        {
            path.Add(step.Value);
            step = previous[step.Value];
        }
        path.Reverse();

        return path;
    }

    // Returns the simulated travel time for a drone to cover the given distance.
    public TimeSpan GetTravelTime(double distanceMiles)
    {
        var hours = distanceMiles / DroneSpeedMph;

        return TimeSpan.FromHours(hours);
    }
}
