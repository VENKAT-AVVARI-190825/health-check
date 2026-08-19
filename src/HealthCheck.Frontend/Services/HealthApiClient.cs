using System.Net.Http.Json;
using HealthCheck.Frontend.Models;

namespace HealthCheck.Frontend.Services;

public sealed class HealthApiClient(HttpClient httpClient)
{
    public async Task<HealthStatusResponse?> GetHealthAsync(CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetAsync("/api/health", cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<HealthStatusResponse>(cancellationToken: cancellationToken);
    }
}
