using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PackageTracker.Accessors;
using PackageTracker.Accessors.Data;
using PackageTracker.Accessors.Interfaces;
using PackageTracker.Engines;
using PackageTracker.Managers;

var builder = WebApplication.CreateBuilder(args);

////
//// Dependency Injection
////

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
builder.Services.AddScoped<IPackageStatusEventAccessor, PackageStatusEventAccessor>();

// Engines
builder.Services.AddScoped<IAuthEngine, AuthEngine>();
builder.Services.AddScoped<IRequestEngine, RequestEngine>();
builder.Services.AddScoped<IRoutingEngine, RoutingEngine>();
builder.Services.AddScoped<IUserTrackingEngine, UserTrackingEngine>();
builder.Services.AddScoped<IStaffTrackingEngine, StaffTrackingEngine>();
builder.Services.AddScoped<ISimulationEngine, SimulationEngine>();
builder.Services.AddHostedService<SimulationBackgroundService>();

// JWT Authentication
var jwtSecret = builder.Configuration["Jwt:Secret"] ?? throw new InvalidOperationException("JWT secret is not configured.");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = builder.Configuration["Jwt:Issuer"],
            ValidAudience            = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
        };
    });

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddControllers();

builder.Services.AddCors(options => {
    options.AddPolicy("AllowAngular", policy => {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseCors("AllowAngular");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
