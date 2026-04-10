using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PackageTracker.Engines;
using PackageTracker.Managers.Dtos;
using PackageTracker.Managers.Interfaces;

namespace PackageTracker.Managers.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthEngine authEngine) : ControllerBase, IAuthManager
{
    // POST api/auth/login
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        try
        {
            var token = await authEngine.Login(request.Email, request.Password);
            return Ok(new { token });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
    }

    // POST api/auth/logout
    // JWT is stateless — the client is responsible for discarding the token.
    [Authorize]
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        return Ok("Logged out successfully.");
    }

    // GET api/auth/role?token=...
    [HttpGet("role")]
    public IActionResult GetUserRole([FromQuery] string token)
    {
        try
        {
            var role = authEngine.GetUserRole(token);
            return Ok(new { role });
        }
        catch (Exception ex)
        {
            return Unauthorized(ex.Message);
        }
    }

    // GET api/auth/validate?token=...
    [HttpGet("validate")]
    public IActionResult ValidateToken([FromQuery] string token)
    {
        var isValid = authEngine.ValidateToken(token);
        return Ok(isValid);
    }
}
