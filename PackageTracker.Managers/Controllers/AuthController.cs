using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using PackageTracker.Accessors.Interfaces;
using PackageTracker.Managers.Dtos;
using PackageTracker.Managers.Interfaces;

namespace PackageTracker.Managers.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IUserAccessor userAccessor, IConfiguration configuration) : ControllerBase, IAuthManager
{
    // POST api/auth/login
    // Validates email/password against the User table and returns a signed JWT on success.
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        var user = await userAccessor.GetByEmail(request.Email);

        // TODO: replace plain-text comparison with a hashed password check (e.g. BCrypt) once
        // password hashing is added to the registration flow.
        if (user == null || user.Password != request.Password)
            return Unauthorized("Invalid email or password.");

        var token = GenerateJwtToken(user.Id, user.Email, user.Role.ToString());

        return Ok(new { token });
    }

    // POST api/auth/logout
    // JWT is stateless — the server cannot invalidate a token without a blacklist.
    // The client is responsible for discarding the token on logout.
    [Authorize]
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        return Ok("Logged out successfully.");
    }

    // GET api/auth/role
    // Reads the role claim directly from the validated JWT — no DB lookup required.
    [Authorize]
    [HttpGet("role")]
    public IActionResult GetUserRole()
    {
        var role = User.FindFirstValue(ClaimTypes.Role);

        if (role == null)
            return Unauthorized("No role claim found in token.");

        return Ok(new { role });
    }

    // GET api/auth/validate
    // The [Authorize] attribute validates the token before the method body runs.
    // Reaching this point means the token is valid and not expired.
    [Authorize]
    [HttpGet("validate")]
    public IActionResult ValidateToken()
    {
        return Ok(true);
    }

    private string GenerateJwtToken(int userId, string email, string role)
    {
        var secret = configuration["Jwt:Secret"] ?? throw new InvalidOperationException("JWT secret is not configured.");
        var key    = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds  = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(ClaimTypes.Role, role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer:   configuration["Jwt:Issuer"],
            audience: configuration["Jwt:Audience"],
            claims:   claims,
            expires:  DateTime.UtcNow.AddHours(24),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
