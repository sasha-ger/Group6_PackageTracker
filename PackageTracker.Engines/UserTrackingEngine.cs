using PackageTracker.Accessors.Interfaces;
using PackageTracker.Models;
using PackageTracker.Models.Enums;

namespace PackageTracker.Engines;

public class UserTrackingEngine(IPackageAccessor packageAccessor) : IUserTrackingEngine
{
    // Fetches the package by ID and returns a status string via BuildStatusString.
    // Throws if the package does not exist.
    public async Task<string> GetPackageStatus(int packageId)
    {
        var package = await packageAccessor.GetById(packageId)
            ?? throw new Exception($"Package {packageId} not found.");

        return BuildStatusString(package);
    }

    // Fetches the package by ID and returns a formatted string with its full details:
    // tracking number, recipient, origin, destination, status, and last updated timestamp.
    // Throws if the package does not exist.
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

    // Returns all packages sent by the given customer ID.
    public async Task<List<Package>> GetPackagesByCustomer(int userId)
    {
        return await packageAccessor.GetBySenderId(userId);
    }

    // Converts a Package's status enum into a customer-facing sentence.
    // Used by GetPackageStatus and can be called directly when a Package object is already in scope.
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
