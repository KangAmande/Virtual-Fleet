namespace Shared;

public class TelemetryPayload
{
    public string VehicleId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double SpeedKph { get; set; }
    public double EngineTemperatureC { get; set; }
    public string? FaultCode { get; set; }
}