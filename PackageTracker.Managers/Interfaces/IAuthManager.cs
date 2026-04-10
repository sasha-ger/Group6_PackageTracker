using Microsoft.AspNetCore.Mvc;
using PackageTracker.Managers.Dtos;

namespace PackageTracker.Managers.Interfaces;

public interface IAuthManager
{
    // Validates credentials and returns a JWT token on success
    Task<IActionResult> Login(LoginRequestDto request);

    // Ends the session — client should discard the token
    IActionResult Logout();

    // Returns the role (customer or staff) encoded in the token — no DB lookup
    IActionResult GetUserRole();

    // Checks that the token in the Authorization header is valid and not expired
    IActionResult ValidateToken();
}
