using Microsoft.EntityFrameworkCore;

namespace IngestionAPI.Data;

public class FleetDbContext : DbContext
{
    public DbSet<TelemetryRecord> TelemetryRecords => Set<TelemetryRecord>();

    public FleetDbContext(DbContextOptions<FleetDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<TelemetryRecord>(entity =>
        {
            entity.HasKey(e => new { e.VehicleId, e.Timestamp });
            entity.ToTable("telemetry");
        });
    }
}

public class TelemetryRecord
{
    public string VehicleId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double SpeedKph { get; set; }
    public double EngineTemperatureC { get; set; }
    public string? FaultCode { get; set; }
}