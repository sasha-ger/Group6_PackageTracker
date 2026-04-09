# PackageTracker

Group Members: Sasha Gerasimov, Subbu Kundoor, Kanwal Lotay, Emma Rhode, Aden Smith

## Database Setup
This project uses MS SQL Server with Entity Framework Core.

1. Install SQL Server locally or use a Docker container
2. Create your local config files by copying the example files and filling in your credentials:
   - Copy `PackageTracker.Managers/appsettings.example.json` → `PackageTracker.Managers/appsettings.json`
   - Copy `PackageTracker.Accessors/appsettings.example.json` → `PackageTracker.Accessors/appsettings.json`
3. Install the EF Core CLI tools if you haven't already:
   ```bash
   dotnet tool install --global dotnet-ef
   ```
4. Apply the migrations to create your local database:
   ```bash
   dotnet ef database update --project PackageTracker.Accessors --startup-project PackageTracker.Managers
   ```
