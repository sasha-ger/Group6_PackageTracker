using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using PackageTracker.Accessors.Interfaces;
using PackageTracker.Managers.Controllers;
using PackageTracker.Managers.Dtos;
using PackageTracker.Models;
using PackageTracker.Models.Enums;

namespace PackageTracker.Tests;

public class AuthControllerTests
{
    private readonly Mock<IUserAccessor> _mockUserAccessor = new();
    private readonly Mock<IConfiguration> _mockConfig = new();
    private readonly AuthController _controller;

    private static readonly User TestUser = new()
    {
        Id = 1,
        Email = "jane@example.com",
        Password = "password123",
        Username = "jane",
        Firstname = "Jane",
        Lastname = "Doe",
        Role = UserRole.Customer
    };

    public AuthControllerTests()
    {
        _mockConfig.Setup(c => c["Jwt:Secret"]).Returns("super-secret-test-key-that-is-32-chars!!");
        _mockConfig.Setup(c => c["Jwt:Issuer"]).Returns("PackageTrackerApi");
        _mockConfig.Setup(c => c["Jwt:Audience"]).Returns("PackageTrackerClient");

        _controller = new AuthController(_mockUserAccessor.Object, _mockConfig.Object);
    }

    // Helper to set up a ClaimsPrincipal on the controller's HttpContext
    private void SetUserClaims(int userId, string role)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Role, role)
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };
    }

    private void SetUserClaimsWithoutRole()
    {
        var identity = new ClaimsIdentity(new List<Claim>(), "TestAuth");
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };
    }

    // --- Login ---

    [Fact]
    public async Task Login_ValidCredentials_ReturnsOkWithToken()
    {
        _mockUserAccessor.Setup(a => a.GetByEmail("jane@example.com")).ReturnsAsync(TestUser);

        var result = await _controller.Login(new LoginRequestDto { Email = "jane@example.com", Password = "password123" });

        var ok = Assert.IsType<OkObjectResult>(result);
        var token = ok.Value?.GetType().GetProperty("token")?.GetValue(ok.Value) as string;
        Assert.NotNull(token);

        // Verify the token is a valid JWT with the expected claims
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        Assert.Equal("jane@example.com", jwt.Claims.First(c => c.Type == JwtRegisteredClaimNames.Email).Value);
        Assert.Equal("Customer", jwt.Claims.First(c => c.Type == ClaimTypes.Role).Value);
    }

    [Fact]
    public async Task Login_UserNotFound_ReturnsUnauthorized()
    {
        _mockUserAccessor.Setup(a => a.GetByEmail(It.IsAny<string>())).ReturnsAsync((User?)null);

        var result = await _controller.Login(new LoginRequestDto { Email = "nobody@example.com", Password = "pass" });

        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public async Task Login_WrongPassword_ReturnsUnauthorized()
    {
        _mockUserAccessor.Setup(a => a.GetByEmail("jane@example.com")).ReturnsAsync(TestUser);

        var result = await _controller.Login(new LoginRequestDto { Email = "jane@example.com", Password = "wrongpassword" });

        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    // --- Logout ---

    [Fact]
    public void Logout_ReturnsOk()
    {
        var result = _controller.Logout();

        Assert.IsType<OkObjectResult>(result);
    }

    // --- GetUserRole ---

    [Fact]
    public void GetUserRole_WithRoleClaim_ReturnsRole()
    {
        SetUserClaims(1, "Staff");

        var result = _controller.GetUserRole();

        var ok = Assert.IsType<OkObjectResult>(result);
        var role = ok.Value?.GetType().GetProperty("role")?.GetValue(ok.Value) as string;
        Assert.Equal("Staff", role);
    }

    [Fact]
    public void GetUserRole_WithoutRoleClaim_ReturnsUnauthorized()
    {
        SetUserClaimsWithoutRole();

        var result = _controller.GetUserRole();

        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    // --- ValidateToken ---

    [Fact]
    public void ValidateToken_ReturnsOk()
    {
        var result = _controller.ValidateToken();

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(true, ok.Value);
    }
}
