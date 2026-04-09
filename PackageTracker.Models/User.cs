using System;
using PackageTracker.Models.Enums;

namespace PackageTracker.Models;
public class User
{
	public int Id { get; set; }
	public string Username { get; set; } = null!;
	public string Firstname { get; set; } = null!;
	public string Lastname { get; set; } = null!;
	public string Email { get; set; } = null!;
	public string Password { get; set; } = null!;
	public UserRole Role { get; set; }
}

