namespace HealthCheck.Frontend.Models;

public sealed record HealthStatusResponse(
    string Status,
    string Service,
    string Environment,
    string Version,
    DateTime Timestamp,
    string[] Dependencies,
    bool IsHealthy);
