using Microsoft.EntityFrameworkCore;
using PackageTracker.Accessors.Data;
using PackageTracker.Accessors.Interfaces;
using PackageTracker.Models;

namespace PackageTracker.Accessors;

public class UserAccessor: IUserAccessor
{
    private readonly AppDbContext _db;

    public UserAccessor(AppDbContext db)
    {
        _db = db;
    }

    public async Task<User?> GetById(int id)
    {
        return await _db.Users.FindAsync(id);
    }

    public async Task<User?> GetByEmail(string email)
    {
        return await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User> Create(User user)
    {
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return user;
    }
}