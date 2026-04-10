using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PackageTracker.Engines;
using PackageTracker.Managers.Controllers;
using PackageTracker.Managers.Dtos;
using PackageTracker.Models;
using PackageTracker.Models.Enums;

namespace PackageTracker.Tests;

public class CustomerControllerTests
{
    private readonly Mock<IRequestEngine> _mockRequestEngine = new();
    private readonly Mock<IUserTrackingEngine> _mockUserTrackingEngine = new();
    private readonly CustomerController _controller;

    private static readonly DeliveryRequestDto ValidRequest = new()
    {
        OriginAddress      = "123 Origin St",
        OriginLat          = 40.0,
        OriginLng          = -75.0,
        DestinationAddress = "456 Dest Ave",
        DestinationLat     = 40.1,
        DestinationLng     = -75.1,
        Recipient          = "Jane Doe"
    };

    private static Package MakePackage(int id) => new()
    {
        Id = id,
        TrackingNumber = $"TRK{id:D3}",
        Recipient = "Jane Doe",
        Status = PackageStatus.InTransit,
        OriginLocation = new Location { Address = "123 Origin St" },
        DestinationLocation = new Location { Address = "456 Dest Ave" },
        CreatedAt = DateTime.UtcNow
    };

    public CustomerControllerTests()
    {
        _controller = new CustomerController(_mockRequestEngine.Object, _mockUserTrackingEngine.Object);

        // Set up a default authenticated user on the HttpContext
        SetUserId(1);
    }

    private void SetUserId(int userId)
    {
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, userId.ToString()) };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };
    }

    // --- CreateDeliveryRequest ---

    [Fact]
    public async Task CreateDeliveryRequest_ValidRequest_ReturnsOk()
    {
        _mockRequestEngine
            .Setup(e => e.ProcessDeliveryRequest(1,
                ValidRequest.OriginAddress,      ValidRequest.OriginLat,      ValidRequest.OriginLng,
                ValidRequest.DestinationAddress, ValidRequest.DestinationLat, ValidRequest.DestinationLng,
                ValidRequest.Recipient))
            .Returns(Task.CompletedTask);

        var result = await _controller.CreateDeliveryRequest(ValidRequest);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task CreateDeliveryRequest_InvalidLocations_ReturnsBadRequest()
    {
        _mockRequestEngine
            .Setup(e => e.ProcessDeliveryRequest(It.IsAny<int>(),
                It.IsAny<string>(), It.IsAny<double>(), It.IsAny<double>(),
                It.IsAny<string>(), It.IsAny<double>(), It.IsAny<double>(),
                It.IsAny<string>()))
            .ThrowsAsync(new ArgumentException("Pickup location is not within range of any depot."));

        var result = await _controller.CreateDeliveryRequest(ValidRequest);

        var bad = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("range", bad.Value?.ToString());
    }

    [Fact]
    public async Task CreateDeliveryRequest_EngineThrowsUnexpectedly_Returns500()
    {
        _mockRequestEngine
            .Setup(e => e.ProcessDeliveryRequest(It.IsAny<int>(),
                It.IsAny<string>(), It.IsAny<double>(), It.IsAny<double>(),
                It.IsAny<string>(), It.IsAny<double>(), It.IsAny<double>(),
                It.IsAny<string>()))
            .ThrowsAsync(new Exception("Unexpected error."));

        var result = await _controller.CreateDeliveryRequest(ValidRequest);

        var serverError = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, serverError.StatusCode);
    }

    // --- GetPackageStatus ---

    [Fact]
    public async Task GetPackageStatus_PackageExists_ReturnsOk()
    {
        _mockUserTrackingEngine.Setup(e => e.GetPackageStatus(1)).ReturnsAsync("Package TRK001 is in transit.");

        var result = await _controller.GetPackageStatus(1);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("Package TRK001 is in transit.", ok.Value);
    }

    [Fact]
    public async Task GetPackageStatus_PackageNotFound_ReturnsNotFound()
    {
        _mockUserTrackingEngine.Setup(e => e.GetPackageStatus(99)).ThrowsAsync(new Exception("Package 99 not found."));

        var result = await _controller.GetPackageStatus(99);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    // --- GetPackagesByCustomer ---

    [Fact]
    public async Task GetPackagesByCustomer_ReturnsPackageList()
    {
        var packages = new List<Package> { MakePackage(1), MakePackage(2) };
        _mockUserTrackingEngine.Setup(e => e.GetPackagesByCustomer(1)).ReturnsAsync(packages);

        var result = await _controller.GetPackagesByCustomer(1);

        var ok = Assert.IsType<OkObjectResult>(result);
        var returned = Assert.IsType<List<Package>>(ok.Value);
        Assert.Equal(2, returned.Count);
    }

    [Fact]
    public async Task GetPackagesByCustomer_NoPackages_ReturnsEmptyList()
    {
        _mockUserTrackingEngine.Setup(e => e.GetPackagesByCustomer(1)).ReturnsAsync(new List<Package>());

        var result = await _controller.GetPackagesByCustomer(1);

        var ok = Assert.IsType<OkObjectResult>(result);
        var returned = Assert.IsType<List<Package>>(ok.Value);
        Assert.Empty(returned);
    }
}
