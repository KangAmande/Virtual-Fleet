using IngestionAPI.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared;

var builder = WebApplication.CreateBuilder(args);

// Add PostgreSQL connection
var connectionString = "Host=localhost;Database=virtualfleet;Username=postgres;Password=password123";
builder.Services.AddDbContext<FleetDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Automatically ensure database and TimescaleDB hypertable exist on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<FleetDbContext>();
    db.Database.EnsureCreated();

    try
    {
        db.Database.ExecuteSqlRaw("SELECT create_hypertable('telemetry', 'timestamp', if_not_exists => TRUE);");
        app.Logger.LogInformation("TimescaleDB hypertable 'telemetry' verified/initialized successfully.");
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning("Hypertable setup note (may already exist): {Message}", ex.Message);
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// POST: Ingest Telemetry Payload
app.MapPost("/api/telemetry", async ([FromBody] TelemetryPayload payload, [FromServices] FleetDbContext db, ILogger<Program> logger) =>
{
    if (string.IsNullOrEmpty(payload.VehicleId))
    {
        return Results.BadRequest(new { error = "VehicleId is required." });
    }

    var record = new TelemetryRecord
    {
        VehicleId = payload.VehicleId,
        Timestamp = payload.Timestamp == default ? DateTime.UtcNow : payload.Timestamp,
        Latitude = payload.Latitude,
        Longitude = payload.Longitude,
        SpeedKph = payload.SpeedKph,
        EngineTemperatureC = payload.EngineTemperatureC,
        FaultCode = payload.FaultCode
    };

    db.TelemetryRecords.Add(record);
    await db.SaveChangesAsync();

    logger.LogInformation("Persisted Telemetry -> Vehicle: {Id} | Speed: {Speed} km/h", payload.VehicleId, payload.SpeedKph);

    return Results.Accepted(string.Empty, new { status = "Persisted", timestamp = DateTime.UtcNow });
});

// GET: Summary list of active vehicles
app.MapGet("/api/vehicles", async ([FromServices] FleetDbContext db) =>
{
    var vehicles = await db.TelemetryRecords
        .GroupBy(t => t.VehicleId)
        .Select(g => new {
            VehicleId = g.Key,
            LastSeen = g.Max(t => t.Timestamp),
            LatestSpeed = g.OrderByDescending(t => t.Timestamp).Select(t => t.SpeedKph).FirstOrDefault(),
            LatestTemp = g.OrderByDescending(t => t.Timestamp).Select(t => t.EngineTemperatureC).FirstOrDefault()
        })
        .ToListAsync();

    return Results.Ok(vehicles);
});

// GET: History for a specific vehicle
app.MapGet("/api/vehicles/{vehicleId}/history", async (string vehicleId, int? limit, [FromServices] FleetDbContext db) =>
{
    int takeCount = limit ?? 20;
    var history = await db.TelemetryRecords
        .Where(t => t.VehicleId == vehicleId)
        .OrderByDescending(t => t.Timestamp)
        .Take(takeCount)
        .ToListAsync();

    if (!history.Any())
    {
        return Results.NotFound(new { error = $"No telemetry history found for vehicle {vehicleId}" });
    }

    return Results.Ok(history);
});

app.Run("http://localhost:5000");