using Microsoft.EntityFrameworkCore;
using PackageTracker.Accessors;
using PackageTracker.Accessors.Data;
using PackageTracker.Accessors.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("PackageTracker.Accessors")));

// Accessors
builder.Services.AddScoped<IPackageAccessor, PackageAccessor>();
builder.Services.AddScoped<IUserAccessor, UserAccessor>();
builder.Services.AddScoped<IDroneAccessor, DroneAccessor>();
builder.Services.AddScoped<IDepotAccessor, DepotAccessor>();
builder.Services.AddScoped<ILocationAccessor, LocationAccessor>();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.Run();
