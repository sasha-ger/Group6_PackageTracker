using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PackageTracker.Engines;
using PackageTracker.Managers.Dtos;
using PackageTracker.Managers.Interfaces;

namespace PackageTracker.Managers.Controllers;

[Authorize]
[ApiController]
[Route("api/customer")]
public class CustomerController(IRequestEngine requestEngine, IUserTrackingEngine userTrackingEngine)
    : ControllerBase, ICustomerManager
{
    // POST api/customer/request
    // Submits a new delivery request. Routes all logic to RequestEngine.
    [HttpPost("request")]
    public async Task<IActionResult> CreateDeliveryRequest([FromBody] DeliveryRequestDto request)
    {
        try
        {
            await requestEngine.ProcessDeliveryRequest(
                GetUserId(),
                request.OriginAddress,      request.OriginLat,      request.OriginLng,
                request.DestinationAddress, request.DestinationLat, request.DestinationLng,
                request.Recipient);

            return Ok("Delivery request submitted successfully.");
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    // GET api/customer/package/{packageId}/status
    // Returns the current human-readable status string for a package.
    [HttpGet("package/{packageId}/status")]
    public async Task<IActionResult> GetPackageStatus(int packageId)
    {
        try
        {
            var status = await userTrackingEngine.GetPackageStatus(packageId);
            return Ok(status);
        }
        catch (Exception ex)
        {
            return NotFound(ex.Message);
        }
    }

    // GET api/customer/{userId}/packages
    // Returns all packages associated with a given customer.
    [HttpGet("{userId}/packages")]
    public async Task<IActionResult> GetPackagesByCustomer(int userId)
    {
        var packages = await userTrackingEngine.GetPackagesByCustomer(userId);
        return Ok(packages);
    }

    // Reads the authenticated user's ID out of their JWT claims.
    private int GetUserId()
    {
        var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("User ID claim not found in token.");
        return int.Parse(claim.Value);
    }
}
