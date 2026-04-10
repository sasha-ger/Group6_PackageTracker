namespace PackageTracker.Engines;

public interface IAuthEngine
{
    // Validates credentials against the User table and returns a signed JWT on success.
    // Throws UnauthorizedAccessException if credentials are invalid.
    Task<string> Login(string email, string password);

    // Decodes the JWT and returns the role claim. Does not hit the database.
    string GetUserRole(string token);

    // Returns true if the JWT signature is valid and the token has not expired.
    bool ValidateToken(string token);
}
