using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using PackageTracker.Accessors.Interfaces;

namespace PackageTracker.Engines;

public class AuthEngine(IUserAccessor userAccessor, IConfiguration configuration) : IAuthEngine
{
    private readonly string _secret   = configuration["Jwt:Secret"]   ?? throw new InvalidOperationException("JWT secret is not configured.");
    private readonly string _issuer   = configuration["Jwt:Issuer"]   ?? "PackageTrackerApi";
    private readonly string _audience = configuration["Jwt:Audience"] ?? "PackageTrackerClient";

    // Looks up the user by email, verifies the password, and returns a signed JWT.
    // TODO: replace plain-text password comparison with hashed check (e.g. BCrypt)
    // once password hashing is added to the registration flow.
    public async Task<string> Login(string email, string password)
    {
        var user = await userAccessor.GetByEmail(email);

        if (user == null || user.Password != password)
            throw new UnauthorizedAccessException("Invalid email or password.");

        var key   = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub,   user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Role,               user.Role.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer:             _issuer,
            audience:           _audience,
            claims:             claims,
            expires:            DateTime.UtcNow.AddHours(24),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    // Decodes the JWT and returns the value of the role claim.
    // Throws if the token is malformed or the role claim is missing.
    public string GetUserRole(string token)
    {
        var jwt  = new JwtSecurityTokenHandler().ReadJwtToken(token);
        var role = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

        return role ?? throw new InvalidOperationException("No role claim found in token.");
    }

    // Validates the token's signature, issuer, audience, and expiry.
    // Returns false for any invalid or expired token instead of throwing.
    public bool ValidateToken(string token)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));

        try
        {
            new JwtSecurityTokenHandler().ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey         = key,
                ValidateIssuer           = true,
                ValidIssuer              = _issuer,
                ValidateAudience         = true,
                ValidAudience            = _audience,
                ValidateLifetime         = true,
                ClockSkew                = TimeSpan.Zero
            }, out _);

            return true;
        }
        catch
        {
            return false;
        }
    }
}
