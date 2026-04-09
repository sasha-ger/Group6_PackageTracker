namespace PackageTracker.Engines;

public interface IRoutingEngine
{
    // Haversine distance between two lat/lng points
    double GetDistance(double lat1, double lng1, double lat2, double lng2);

    // Is a location within 10 miles of any depot
    Task<bool> IsWithinRange(double lat, double lng);

    // Find the closest depot to a location (returns depot ID)
    Task<int> FindNearestDepot(double lat, double lng);

    // Dijkstra's shortest path — returns ordered list of depot IDs
    List<int> FindShortestRoute(int originDepotId, int destinationDepotId);
}
