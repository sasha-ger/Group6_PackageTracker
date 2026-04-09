using Moq;
using PackageTracker.Accessors.Interfaces;
using PackageTracker.Engines;
using PackageTracker.Models;
using PackageTracker.Models.Enums;

namespace PackageTracker.Tests;

public class UserTrackingEngineTests
{
    private readonly Mock<IPackageAccessor> _mockAccessor = new();
    private readonly UserTrackingEngine _engine;

    private static Package MakePackage(int id, PackageStatus status) => new()
    {
        Id = id,
        TrackingNumber = $"TRK{id:D3}",
        Recipient = "Jane Doe",
        Status = status,
        OriginLocation = new Location { Address = "123 Origin St" },
        DestinationLocation = new Location { Address = "456 Dest Ave" },
        CreatedAt = new DateTime(2026, 1, 1),
    };

    public UserTrackingEngineTests()
    {
        _engine = new UserTrackingEngine(_mockAccessor.Object);
    }

    // --- GetPackageStatus ---

    [Fact]
    public async Task GetPackageStatus_Pending_ReturnsCorrectString()
    {
        _mockAccessor.Setup(a => a.GetById(1)).ReturnsAsync(MakePackage(1, PackageStatus.Pending));

        var result = await _engine.GetPackageStatus(1);

        Assert.Equal("Package TRK001 is pending pickup.", result);
    }

    [Fact]
    public async Task GetPackageStatus_InTransit_ReturnsCorrectString()
    {
        _mockAccessor.Setup(a => a.GetById(2)).ReturnsAsync(MakePackage(2, PackageStatus.InTransit));

        var result = await _engine.GetPackageStatus(2);

        Assert.Equal("Package TRK002 is in transit.", result);
    }

    [Fact]
    public async Task GetPackageStatus_Delivered_ReturnsCorrectString()
    {
        _mockAccessor.Setup(a => a.GetById(3)).ReturnsAsync(MakePackage(3, PackageStatus.Delivered));

        var result = await _engine.GetPackageStatus(3);

        Assert.Equal("Package TRK003 has been delivered.", result);
    }

    [Fact]
    public async Task GetPackageStatus_Failed_ReturnsCorrectString()
    {
        _mockAccessor.Setup(a => a.GetById(4)).ReturnsAsync(MakePackage(4, PackageStatus.Failed));

        var result = await _engine.GetPackageStatus(4);

        Assert.Equal("Package TRK004 could not be delivered.", result);
    }

    [Fact]
    public async Task GetPackageStatus_PackageNotFound_ThrowsException()
    {
        _mockAccessor.Setup(a => a.GetById(99)).ReturnsAsync((Package?)null);

        await Assert.ThrowsAsync<Exception>(() => _engine.GetPackageStatus(99));
    }

    // --- GetPackageDetails ---

    [Fact]
    public async Task GetPackageDetails_ReturnsAllFields()
    {
        _mockAccessor.Setup(a => a.GetById(1)).ReturnsAsync(MakePackage(1, PackageStatus.InTransit));

        var result = await _engine.GetPackageDetails(1);

        Assert.Contains("TRK001", result);
        Assert.Contains("Jane Doe", result);
        Assert.Contains("123 Origin St", result);
        Assert.Contains("456 Dest Ave", result);
        Assert.Contains("InTransit", result);
        Assert.Contains("N/A", result); // UpdatedAt is null
    }

    [Fact]
    public async Task GetPackageDetails_WithUpdatedAt_ShowsDate()
    {
        var package = MakePackage(1, PackageStatus.Delivered);
        package.UpdatedAt = new DateTime(2026, 3, 15, 10, 30, 0);
        _mockAccessor.Setup(a => a.GetById(1)).ReturnsAsync(package);

        var result = await _engine.GetPackageDetails(1);

        Assert.DoesNotContain("N/A", result);
        Assert.Contains("3/15/2026", result);
    }

    [Fact]
    public async Task GetPackageDetails_PackageNotFound_ThrowsException()
    {
        _mockAccessor.Setup(a => a.GetById(99)).ReturnsAsync((Package?)null);

        await Assert.ThrowsAsync<Exception>(() => _engine.GetPackageDetails(99));
    }

    // --- BuildStatusString ---

    [Theory]
    [InlineData(PackageStatus.Pending, "pending pickup")]
    [InlineData(PackageStatus.InTransit, "in transit")]
    [InlineData(PackageStatus.Delivered, "has been delivered")]
    [InlineData(PackageStatus.Failed, "could not be delivered")]
    public void BuildStatusString_AllStatuses_ContainExpectedText(PackageStatus status, string expectedFragment)
    {
        var package = MakePackage(1, status);

        var result = _engine.BuildStatusString(package);

        Assert.Contains(expectedFragment, result);
        Assert.Contains("TRK001", result);
    }
}
