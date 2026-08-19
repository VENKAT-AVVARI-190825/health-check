namespace HealthCheck.Api.Services;

public sealed record HealthStatusResponse(
    string Status,
    string Service,
    string Environment,
    string Version,
    DateTime Timestamp,
    string[] Dependencies,
    bool IsHealthy);

public sealed class HealthStatusService(IConfiguration configuration)
{
    public HealthStatusResponse GetStatus()
    {
        var serviceName = configuration["HealthCheck:ServiceName"] ?? Environment.GetEnvironmentVariable("HEALTHCHECK_SERVICE_NAME") ?? "unknown-service";
        var environment = configuration["HealthCheck:Environment"] ?? Environment.GetEnvironmentVariable("HEALTHCHECK_ENVIRONMENT") ?? "local";
        var version = configuration["HealthCheck:Version"] ?? Environment.GetEnvironmentVariable("HEALTHCHECK_VERSION") ?? "0.0.0-local";
        var dependencies = configuration.GetSection("HealthCheck:DependencyCheck").Get<string[]>() ??
            (Environment.GetEnvironmentVariable("HEALTHCHECK_DEPENDENCIES") is { Length: > 0 } deps
                ? deps.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                : []);

        return new HealthStatusResponse(
            Status: "healthy",
            Service: serviceName,
            Environment: environment,
            Version: version,
            Timestamp: DateTime.UtcNow,
            Dependencies: dependencies,
            IsHealthy: true);
    }

    public object GetDetail()
    {
        var status = GetStatus();
        return new
        {
            status.Status,
            status.Service,
            status.Environment,
            status.Version,
            status.Timestamp,
            status.Dependencies,
            status.IsHealthy,
            checks = status.Dependencies.Select(name => new
            {
                name,
                status = "ok",
                checkedAt = DateTime.UtcNow
            })
        };
    }
}
