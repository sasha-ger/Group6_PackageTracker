using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PackageTracker.Engines;
using PackageTracker.Managers.Interfaces;

namespace PackageTracker.Managers.Controllers;

[Authorize(Roles = "Staff")]
[ApiController]
[Route("api/staff")]
public class StaffController(IStaffTrackingEngine staffTrackingEngine)
    : ControllerBase, IStaffManager
{
    // GET api/staff/drones
    // Returns all drones with their current status and depot location.
    [HttpGet("drones")]
    public async Task<IActionResult> GetAllDrones()
    {
        var drones = await staffTrackingEngine.GetAllDroneStatuses();
        return Ok(drones);
    }

    // GET api/staff/drones/by-package/{packageId}
    // Returns the drone currently carrying a given package, or 404 if none.
    [HttpGet("drones/by-package/{packageId}")]
    public async Task<IActionResult> GetDroneByPackage(int packageId)
    {
        var drone = await staffTrackingEngine.GetDroneByPackage(packageId);

        if (drone == null)
            return NotFound($"No drone is currently assigned to package {packageId}.");

        return Ok(drone);
    }

    // GET api/staff/packages/active
    // Returns all packages that have not yet been delivered or failed.
    [HttpGet("packages/active")]
    public async Task<IActionResult> GetAllActivePackages()
    {
        var packages = await staffTrackingEngine.GetAllActivePackages();
        return Ok(packages);
    }
}
