# PackageTracker — Mo's Drones

Group Members: Sasha Gerasimov, Subbu Kundoor, Kanwal Lotay, Emma Rhode, Aden Smith

---

## Prerequisites

Install the following before getting started:

| Tool | Version | Download |
|------|---------|----------|
| .NET SDK | 10.0+ | https://dotnet.microsoft.com/download |
| Node.js | 18+ (LTS recommended) | https://nodejs.org |
| npm | 11.5.1+ (bundled with Node) | — |
| SQL Server | 2019+ or Azure SQL | https://www.microsoft.com/sql-server/sql-server-downloads |
| EF Core CLI | Latest | `dotnet tool install --global dotnet-ef` |

> **SQL Server on Mac/Linux:** Use the [Docker image](https://hub.docker.com/r/microsoft/mssql-server):
> ```bash
> docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourStrong@Passw0rd" \
>   -p 1433:1433 --name sqlserver -d mcr.microsoft.com/mssql/server:2022-latest
> ```

---

## 1. Clone the Repository

```bash
git clone <repo-url>
cd PackageTracker
```

---

## 2. Database Setup

### 2a. Configure connection strings

Copy the example config files and fill in your SQL Server credentials:

```bash
cp PackageTracker.Managers/appsettings.example.json PackageTracker.Managers/appsettings.json
cp PackageTracker.Accessors/appsettings.example.json PackageTracker.Accessors/appsettings.json
```

Edit **both** files and replace the placeholder values:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=PackageTracker;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;"
}
```

For a local SQL Server instance on Windows you can use Windows Authentication instead:

```json
"DefaultConnection": "Server=localhost;Database=PackageTracker;Trusted_Connection=True;TrustServerCertificate=True;"
```

### 2b. Configure JWT (Managers appsettings.json only)

In `PackageTracker.Managers/appsettings.json`, set a JWT secret (must be at least 32 characters):

```json
"Jwt": {
  "Secret": "replace-this-with-a-random-32-char-secret",
  "Issuer": "PackageTrackerApi",
  "Audience": "PackageTrackerClient"
}
```

### 2c. Apply migrations

Install the EF Core CLI tool if you haven't already:

```bash
dotnet tool install --global dotnet-ef
```

Run the migrations to create the database and seed initial data:

```bash
dotnet ef database update --project PackageTracker.Accessors --startup-project PackageTracker.Managers
```

---

## 3. Backend

From the repository root:

```bash
dotnet run --project PackageTracker.Managers
```

The API will be available at:
- HTTP: http://localhost:5064
- HTTPS: https://localhost:7286

Swagger UI (development only): http://localhost:5064/swagger

---

## 4. Frontend

```bash
cd PackageTracker.Client
npm install
npm start
```

The Angular app will be available at http://localhost:4200.

The dev server proxies API calls to the backend at `http://localhost:5064`.

---

## 5. Running Tests

**Backend (xUnit):**
```bash
dotnet test PackageTracker.Tests
```

**Frontend (Vitest):**
```bash
cd PackageTracker.Client
npm test
```

---

## Project Structure

```
PackageTracker/
├── PackageTracker.Managers/    # ASP.NET Core Web API (entry point)
├── PackageTracker.Accessors/   # Entity Framework Core data layer & migrations
├── PackageTracker.Engines/     # Business logic
├── PackageTracker.Models/      # Shared domain models & enums
├── PackageTracker.Client/      # Angular 21 frontend
└── PackageTracker.Tests/       # xUnit backend tests
```

---

## Project Description

Mo's Drones is a courier service using small unmanned aerial systems (SUAS) to deliver packages within the Lincoln and Omaha, Nebraska, areas. When a customer needs a package to be delivered, a SUAS is dispatched from a nearby depot to pick up the package.

Because the SUAS has a limited range, there are depots every ten miles along I-80 between Seward and the Missouri River. There are also depots in Lincoln at the intersection of O Street and 27th Street, at the intersection of O Street and 84th Street, and at the intersection of 84th Street and Nebraska Highway 2. When a SUAS with a package arrives at a depot, the package is handed off to another SUAS which will carry the package to the next depot or the destination (if the destination is within range).

A customer (both the sender and the receiver) should be able to observe the status of a delivery, to include the point of origin and the destination, when the SUAS was dispatched, when the SUAS picked up the package, when the package was handed off to another SUAS at each depot visited, and when the package was delivered. A customer should also be able to generate a delivery request, which will cause a SUAS to be dispatched automatically to pick up the package.

The Mo's Drones staff should be able to observe where each SUAS is, whether at a depot, between depots, en route to/from a customer, or at a customer's location. The staff should be able to observe which package is aboard which SUAS. Just as the customers can, the staff should be able to observe a package's status. While dispatching a SUAS to pick up a package is automatic, the staff should be able to dispatch an empty SUAS from one depot to another.

Current information about the SUAS & package locations & destinations must be recoverable after a power outage.

The system may be implemented in text-mode or GUI-mode.

**Customers** can track delivery status (origin, destination, dispatch time, pickup, each handoff, and delivery) and submit new delivery requests.

**Staff** can view SUAS locations (depot, en route, at customer), see which package is on which drone, track package status, and manually dispatch empty drones between depots.

