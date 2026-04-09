using PackageTracker.Models;

namespace PackageTracker.Accessors.Interfaces;

public interface IUserAccessor
{
    Task<User?> GetById(int id);
    Task<User?> GetByEmail(string email);
    Task<User> Create(User user);
}
