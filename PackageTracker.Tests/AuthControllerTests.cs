using Microsoft.AspNetCore.Mvc;
using Moq;
using PackageTracker.Engines;
using PackageTracker.Managers.Controllers;
using PackageTracker.Managers.Dtos;

namespace PackageTracker.Tests;

public class AuthControllerTests
{
    private readonly Mock<IAuthEngine> _mockAuthEngine = new();
    private readonly AuthController _controller;

    private const string ValidToken = "valid.jwt.token";

    public AuthControllerTests()
    {
        _controller = new AuthController(_mockAuthEngine.Object);
    }

    // --- Login ---

    [Fact]
    public async Task Login_ValidCredentials_ReturnsOkWithToken()
    {
        _mockAuthEngine.Setup(e => e.Login("jane@example.com", "password123")).ReturnsAsync(ValidToken);

        var result = await _controller.Login(new LoginRequestDto { Email = "jane@example.com", Password = "password123" });

        var ok = Assert.IsType<OkObjectResult>(result);
        var token = ok.Value?.GetType().GetProperty("token")?.GetValue(ok.Value) as string;
        Assert.Equal(ValidToken, token);
    }

    [Fact]
    public async Task Login_InvalidCredentials_ReturnsUnauthorized()
    {
        _mockAuthEngine.Setup(e => e.Login(It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new UnauthorizedAccessException("Invalid email or password."));

        var result = await _controller.Login(new LoginRequestDto { Email = "bad@example.com", Password = "wrong" });

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
    public void GetUserRole_ValidToken_ReturnsRole()
    {
        _mockAuthEngine.Setup(e => e.GetUserRole(ValidToken)).Returns("Staff");

        var result = _controller.GetUserRole(ValidToken);

        var ok = Assert.IsType<OkObjectResult>(result);
        var role = ok.Value?.GetType().GetProperty("role")?.GetValue(ok.Value) as string;
        Assert.Equal("Staff", role);
    }

    [Fact]
    public void GetUserRole_EngineThrows_ReturnsUnauthorized()
    {
        _mockAuthEngine.Setup(e => e.GetUserRole(It.IsAny<string>()))
            .Throws(new InvalidOperationException("No role claim found in token."));

        var result = _controller.GetUserRole("bad.token");

        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    // --- ValidateToken ---

    [Fact]
    public void ValidateToken_ValidToken_ReturnsOkTrue()
    {
        _mockAuthEngine.Setup(e => e.ValidateToken(ValidToken)).Returns(true);

        var result = _controller.ValidateToken(ValidToken);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(true, ok.Value);
    }

    [Fact]
    public void ValidateToken_InvalidToken_ReturnsOkFalse()
    {
        _mockAuthEngine.Setup(e => e.ValidateToken("bad.token")).Returns(false);

        var result = _controller.ValidateToken("bad.token");

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(false, ok.Value);
    }
}
