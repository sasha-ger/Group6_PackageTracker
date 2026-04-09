using PackageTracker.Models;

namespace PackageTracker.Engines;

public interface IUserTrackingEngine
{
    Task<string> GetPackageStatus(int packageId);
    Task<string> GetPackageDetails(int packageId);
    string BuildStatusString(Package package);
}
