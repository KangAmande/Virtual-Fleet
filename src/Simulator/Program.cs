using System.Net.Http.Json;
using Shared;

var client = new HttpClient { BaseAddress = new Uri("http://localhost:5000") };
var random = new Random();

// Simulate a small fleet of 3 vehicles
var activeVehicles = new List<string> { "TRUCK-001", "VAN-042", "SEDAN-109" };

Console.WriteLine("VirtualFleet Telemetry Simulator started. Streaming data to API...");

while (true)
{
    foreach (var vehicleId in activeVehicles)
    {
        var payload = new TelemetryPayload
        {
            VehicleId = vehicleId,
            Timestamp = DateTime.UtcNow,
            Latitude = 43.6532 + (random.NextDouble() - 0.5) * 0.01, // Around a base coordinate
            Longitude = -79.3832 + (random.NextDouble() - 0.5) * 0.01,
            SpeedKph = Math.Round(random.NextDouble() * 110, 2),
            EngineTemperatureC = Math.Round(80 + random.NextDouble() * 25, 2),
            FaultCode = random.Next(1, 100) > 90 ? "P0300" : null // 10% chance of a fault code
        };

        try
        {
            var response = await client.PostAsJsonAsync("/api/telemetry", payload);
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine($"[Sent] {vehicleId} -> Speed: {payload.SpeedKph} km/h");
            }
            else
            {
                Console.WriteLine($"[Error] Failed to send telemetry for {vehicleId}. Status: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Connection Error] Could not reach Ingestion API: {ex.Message}");
        }
    }

    // Wait 2 seconds before sending the next batch
    await Task.Delay(2000);
}