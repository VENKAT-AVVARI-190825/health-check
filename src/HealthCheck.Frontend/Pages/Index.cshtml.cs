using HealthCheck.Frontend.Models;
using HealthCheck.Frontend.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HealthCheck.Frontend.Pages;

public sealed class IndexModel(HealthApiClient healthApiClient, IConfiguration configuration) : PageModel
{
    public HealthStatusResponse? ApiHealth { get; private set; }

    public string ApiBaseUrl { get; private set; } = string.Empty;

    public string ErrorMessage { get; private set; } = string.Empty;

    public async Task OnGetAsync()
    {
        ApiBaseUrl = configuration["HealthApi:BaseUrl"] ?? "http://localhost:5093";

        try
        {
            ApiHealth = await healthApiClient.GetHealthAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
    }
}
