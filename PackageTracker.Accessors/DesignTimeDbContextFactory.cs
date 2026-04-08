using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace PackageTracker.Accessors
{
    // EF tools will use this to create the DbContext when running migrations from the CLI
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<PackageTrackerDbContext>
    {
        public PackageTrackerDbContext CreateDbContext(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .AddEnvironmentVariables();

            var config = builder.Build();
            var conn = config.GetConnectionString("DefaultConnection") ??
                       "Server=(localdb)\\mssqllocaldb;Database=PackageTrackerDb;Trusted_Connection=True;MultipleActiveResultSets=true";

            var optionsBuilder = new DbContextOptionsBuilder<PackageTrackerDbContext>();
            optionsBuilder.UseSqlServer(conn);

            return new PackageTrackerDbContext(optionsBuilder.Options);
        }
    }
}
