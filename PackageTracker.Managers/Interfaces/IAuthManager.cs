using Microsoft.AspNetCore.Mvc;
using PackageTracker.Managers.Dtos;

namespace PackageTracker.Managers.Interfaces;

public interface IAuthManager
{
    // Validates credentials and returns a JWT token on success
    Task<IActionResult> Login(LoginRequestDto request);

    // Ends the session — client should discard the token
    IActionResult Logout();

    // Decodes the given token and returns the role claim — no DB lookup
    IActionResult GetUserRole(string token);

    // Returns true if the given token is valid and not expired
    IActionResult ValidateToken(string token);
}
