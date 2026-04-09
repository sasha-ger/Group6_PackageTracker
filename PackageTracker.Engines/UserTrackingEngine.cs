using PackageTracker.Accessors.Interfaces;
using PackageTracker.Models;
using PackageTracker.Models.Enums;

namespace PackageTracker.Engines;

public class UserTrackingEngine(IPackageAccessor packageAccessor) : IUserTrackingEngine
{
    public async Task<string> GetPackageStatus(int packageId)
    {
        var package = await packageAccessor.GetById(packageId)
            ?? throw new Exception($"Package {packageId} not found.");

        return BuildStatusString(package);
    }

    public async Task<string> GetPackageDetails(int packageId)
    {
        var package = await packageAccessor.GetById(packageId)
            ?? throw new Exception($"Package {packageId} not found.");

        return $"Tracking Number: {package.TrackingNumber}\n" +
               $"Recipient: {package.Recipient}\n" +
               $"Origin: {package.OriginLocation.Address}\n" +
               $"Destination: {package.DestinationLocation.Address}\n" +
               $"Status: {package.Status}\n" +
               $"Last Updated: {(package.UpdatedAt.HasValue ? package.UpdatedAt.Value.ToString("g") : "N/A")}";
    }

    public string BuildStatusString(Package package)
    {
        return package.Status switch
        {
            PackageStatus.Pending    => $"Package {package.TrackingNumber} is pending pickup.",
            PackageStatus.InTransit  => $"Package {package.TrackingNumber} is in transit.",
            PackageStatus.Delivered  => $"Package {package.TrackingNumber} has been delivered.",
            PackageStatus.Failed     => $"Package {package.TrackingNumber} could not be delivered.",
            _                        => $"Package {package.TrackingNumber} has an unknown status."
        };
    }
}
