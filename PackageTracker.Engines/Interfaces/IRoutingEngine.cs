namespace PackageTracker.Engines;

public interface IRoutingEngine
{
    double GetDistance(double lat1, double lng1, double lat2, double lng2);
    Task<bool> IsWithinRange(double lat, double lng);
    Task<int> FindNearestDepot(double lat, double lng);
    Task<List<int>> FindShortestRoute(int originDepotId, int destinationDepotId);
    TimeSpan GetTravelTime(double distanceMiles);
}
