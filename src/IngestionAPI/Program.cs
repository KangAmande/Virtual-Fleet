using Microsoft.AspNetCore.Mvc;
using Shared;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapPost("/api/telemetry", ([FromBody] TelemetryPayload payload, ILogger<Program> logger) =>
{
    // Basic validation check
    if (string.IsNullOrEmpty(payload.VehicleId))
    {
        return Results.BadRequest(new { error = "VehicleId is required." });
    }

    // Log ingestion (Simulating a database write)
    logger.LogInformation("Received Telemetry -> Vehicle: {Id} | Lat: {Lat}, Lon: {Lon} | Speed: {Speed} km/h", 
        payload.VehicleId, payload.Latitude, payload.Longitude, payload.SpeedKph);

    return Results.Accepted(string.Empty, new { status = "Ingested", timestamp = DateTime.UtcNow });
});

app.Run("http://localhost:5000");