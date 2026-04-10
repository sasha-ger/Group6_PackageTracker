using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Moq;
using PackageTracker.Accessors.Interfaces;
using PackageTracker.Engines;
using PackageTracker.Models;
using PackageTracker.Models.Enums;

namespace PackageTracker.Tests;

public class AuthEngineTests
{
    private readonly Mock<IUserAccessor> _mockUserAccessor = new();
    private readonly Mock<IConfiguration> _mockConfig = new();
    private readonly AuthEngine _engine;

    private const string Secret   = "super-secret-test-key-that-is-32-chars!!";
    private const string Issuer   = "PackageTrackerApi";
    private const string Audience = "PackageTrackerClient";

    private static readonly User TestUser = new()
    {
        Id        = 1,
        Email     = "jane@example.com",
        Password  = "password123",
        Username  = "jane",
        Firstname = "Jane",
        Lastname  = "Doe",
        Role      = UserRole.Customer
    };

    public AuthEngineTests()
    {
        _mockConfig.Setup(c => c["Jwt:Secret"]).Returns(Secret);
        _mockConfig.Setup(c => c["Jwt:Issuer"]).Returns(Issuer);
        _mockConfig.Setup(c => c["Jwt:Audience"]).Returns(Audience);

        _engine = new AuthEngine(_mockUserAccessor.Object, _mockConfig.Object);
    }

    // Generates a real signed JWT with known claims for use in tests
    private string MakeToken(string role = "Customer", bool expired = false, bool tampered = false)
    {
        var key    = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tampered ? "wrong-key-wrong-key-wrong-key-!!!" : Secret));
        var creds  = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiry = expired ? DateTime.UtcNow.AddHours(-1) : DateTime.UtcNow.AddHours(24);

        var token = new JwtSecurityToken(
            issuer:             Issuer,
            audience:           Audience,
            claims:             new[] { new Claim(ClaimTypes.Role, role) },
            expires:            expiry,
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    // --- Login ---

    [Fact]
    public async Task Login_ValidCredentials_ReturnsJwtWithCorrectClaims()
    {
        _mockUserAccessor.Setup(a => a.GetByEmail("jane@example.com")).ReturnsAsync(TestUser);

        var token = await _engine.Login("jane@example.com", "password123");

        Assert.NotNull(token);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        Assert.Equal("jane@example.com", jwt.Claims.First(c => c.Type == JwtRegisteredClaimNames.Email).Value);
        Assert.Equal("1",                jwt.Claims.First(c => c.Type == JwtRegisteredClaimNames.Sub).Value);
        Assert.Equal("Customer",         jwt.Claims.First(c => c.Type == ClaimTypes.Role).Value);
    }

    [Fact]
    public async Task Login_UserNotFound_ThrowsUnauthorizedAccessException()
    {
        _mockUserAccessor.Setup(a => a.GetByEmail(It.IsAny<string>())).ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _engine.Login("nobody@example.com", "pass"));
    }

    [Fact]
    public async Task Login_WrongPassword_ThrowsUnauthorizedAccessException()
    {
        _mockUserAccessor.Setup(a => a.GetByEmail("jane@example.com")).ReturnsAsync(TestUser);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _engine.Login("jane@example.com", "wrongpassword"));
    }

    [Fact]
    public async Task Login_StaffUser_TokenContainsStaffRole()
    {
        var staffUser = new User { Id = 2, Email = "bob@example.com", Password = "pass", Role = UserRole.Staff, Username = "bob", Firstname = "Bob", Lastname = "Smith" };
        _mockUserAccessor.Setup(a => a.GetByEmail("bob@example.com")).ReturnsAsync(staffUser);

        var token = await _engine.Login("bob@example.com", "pass");

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        Assert.Equal("Staff", jwt.Claims.First(c => c.Type == ClaimTypes.Role).Value);
    }

    // --- GetUserRole ---

    [Fact]
    public void GetUserRole_ValidToken_ReturnsRole()
    {
        var token = MakeToken("Staff");

        var role = _engine.GetUserRole(token);

        Assert.Equal("Staff", role);
    }

    [Fact]
    public void GetUserRole_CustomerToken_ReturnsCustomer()
    {
        var token = MakeToken("Customer");

        var role = _engine.GetUserRole(token);

        Assert.Equal("Customer", role);
    }

    [Fact]
    public void GetUserRole_TokenWithNoRoleClaim_ThrowsInvalidOperationException()
    {
        // Build a token with no role claim
        var key   = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Secret));
        var token = new JwtSecurityToken(
            issuer:             Issuer,
            audience:           Audience,
            claims:             new[] { new Claim(JwtRegisteredClaimNames.Sub, "1") },
            expires:            DateTime.UtcNow.AddHours(24),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));
        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        Assert.Throws<InvalidOperationException>(() => _engine.GetUserRole(tokenString));
    }

    // --- ValidateToken ---

    [Fact]
    public void ValidateToken_ValidToken_ReturnsTrue()
    {
        var token = MakeToken();

        var result = _engine.ValidateToken(token);

        Assert.True(result);
    }

    [Fact]
    public void ValidateToken_ExpiredToken_ReturnsFalse()
    {
        var token = MakeToken(expired: true);

        var result = _engine.ValidateToken(token);

        Assert.False(result);
    }

    [Fact]
    public void ValidateToken_TamperedToken_ReturnsFalse()
    {
        var token = MakeToken(tampered: true);

        var result = _engine.ValidateToken(token);

        Assert.False(result);
    }

    [Fact]
    public void ValidateToken_GarbageString_ReturnsFalse()
    {
        var result = _engine.ValidateToken("not.a.token");

        Assert.False(result);
    }
}
