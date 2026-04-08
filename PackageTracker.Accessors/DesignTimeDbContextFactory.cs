using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using PackageTracker.Accessors.Data;

namespace PackageTracker.Accessors;
// EF tools will use this to create the DbContext when running migrations from the CLI
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true);

        var config = builder.Build();
        var conn = config.GetConnectionString("DefaultConnection") ??
                "Server=localhost,1433;Database=PackageTrackerDB;User Id=sa;Password=changeme;TrustServerCertificate=True;";

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlServer(conn);

        return new AppDbContext(optionsBuilder.Options);
    }
}

